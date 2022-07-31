using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace MasterCsharpHosted.Server.Services;

public class CodeCompletion
{
    public static async Task<List<CustomSuggestion>> GetCodeCompletion(SourceInfo sourceInfo, List<PortableExecutableReference> refs)
    {
        //List<PortableExecutableReference> refs = CompileResources.PortableExecutableCompletionReferences;

        List<string> usings = new() {"System",
            "System.IO",
            "System.Collections.Generic",
            "System.Collections",
            "System.Console",
            "System.Diagnostics",
            "System.Dynamic",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Net.Http",
            "System.Text",
            "System.Net",
            "System.Threading.Tasks",
            "System.Numerics"};
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && File.Exists(a.Location) && !a.FullName.Contains("JSInterop.WebAssembly")).ToList();

        var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
            .Distinct()?
            .SelectMany(x => x?.GetTypes())?
            .ToArray();

        var compositionContext = new ContainerConfiguration()
            .WithParts(partTypes)
            .CreateContainer();
        var host = MefHostServices.Create(compositionContext);

        var workspace = new AdhocWorkspace(host);

        var scriptCode = sourceInfo.SourceCode;
        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);
        if (refs == null || refs.Count == 0) return null;
         
        var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
            .WithMetadataReferences(refs)
            .WithCompilationOptions(compilationOptions);

        var scriptProject = workspace.AddProject(scriptProjectInfo);
        var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(scriptProject.Id), "Script",
            sourceCodeKind: SourceCodeKind.Script,
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
        var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

        // cursor position is at the end
        var position = sourceInfo.LineNumberOffsetFromTemplate;
        var completionService = CompletionService.GetService(scriptDocument);
        var results = await completionService.GetCompletionsAsync(scriptDocument, position);
        if (results == null && sourceInfo.LineNumberOffsetFromTemplate < sourceInfo.SourceCode.Length)
        {
            sourceInfo.LineNumberOffsetFromTemplate++;
            await GetCodeCompletion(sourceInfo, refs);
        }

        if (sourceInfo.SourceCode[sourceInfo.LineNumberOffsetFromTemplate - 1].ToString() == "(")
        {
            sourceInfo.LineNumberOffsetFromTemplate--;
            results = completionService.GetCompletionsAsync(scriptDocument, sourceInfo.LineNumberOffsetFromTemplate).Result;
        }

        //Method parameters
        var overloads = GetMethodOverloads(scriptCode, position, refs);
        var suggestionList = new List<CustomSuggestion>();
        if (results != null)
        {
            try
            {
                suggestionList.AddRange(results.Items.Select(completion => new CustomSuggestion()
                {
                    Label = completion.Properties.ContainsKey("SymbolName") ? completion.Properties["SymbolName"] : completion.DisplayText,
                    InsertText = completion.Properties.ContainsKey("SymbolName") ? completion.Properties["SymbolName"] : completion.DisplayText,
                    Kind = completion.Properties.ContainsKey("SymbolKind") ? completion.Properties["SymbolKind"] : "8",
                    Detail = completion.Tags != null && completion.Tags.Length > 0 ? completion.Tags[0] : "None",
                    Documentation = completion.Tags != null && completion.Tags.Length > 1 ? completion.Tags[1] : "None"
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error from: \r\n{JsonConvert.SerializeObject(results)}\r\n{ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
        }
            
        if (overloads.Count == 0) return suggestionList;
        suggestionList = new List<CustomSuggestion>();
        var builder = ImmutableArray.CreateBuilder<CompletionItem>();
        foreach (var (description, documentation) in overloads)
        {
            suggestionList.Add(new CustomSuggestion()
            {
                Label = description.Split('(', ')')[1],
                InsertText = description.Split('(', ')')[1],
                Kind = "8",
                Detail = documentation,
                Documentation = documentation
            });
        }
        return suggestionList;
            
    }
    public static SortedList<string, string> GetMethodOverloads(string scriptCode, int position, List<PortableExecutableReference> refs)
    {
        var overloadDocs = new SortedList<string, string>();
        var meta = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        //var assembliesNames = meta.ToString()?.Split(';');
        var sourceLanguage = new CSharpLanguage();
        var syntaxTree = sourceLanguage.ParseText(scriptCode, SourceCodeKind.Script);
        var compilation = CSharpCompilation.Create("MyCompilation",
            new[] { syntaxTree }, refs);

        var model = compilation.GetSemanticModel(syntaxTree);

        var theToken = syntaxTree.GetRoot().FindToken(position);
        var theNode = theToken.Parent;
        while (!theNode.IsKind(SyntaxKind.InvocationExpression))
        {
            theNode = theNode.Parent;
            if (theNode == null) break; // There isn't an InvocationExpression in this branch of the tree
        }

        if (theNode == null) return new SortedList<string, string>();
        var symbolInfo = model.GetSymbolInfo(theNode);

        if (symbolInfo.CandidateSymbols.Length == 0) return overloadDocs;
        var indx = 1;
        foreach (var symb in symbolInfo.CandidateSymbols)
        {
            if(symb.Kind != SymbolKind.Method) continue;
            var valueVal = $"overload {indx}";
            var keyVal = symb.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            overloadDocs.Add(keyVal,valueVal);
            indx++;
        }

        return overloadDocs;


    }
    public class CSharpLanguage : ILanguageService
    {
        private readonly LanguageVersion _maxLanguageVersion = Enum
            .GetValues(typeof(LanguageVersion))
            .Cast<LanguageVersion>()
            .Max();

        public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
        {
            var options = new CSharpParseOptions(kind: kind, languageVersion: _maxLanguageVersion);

            // Return a syntax tree of our source code
            return CSharpSyntaxTree.ParseText(sourceCode, options);
        }
    }
}