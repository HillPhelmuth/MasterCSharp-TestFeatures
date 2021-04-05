using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
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

namespace MasterCsharpHosted.Server.Services
{
    public class CodeCompletion
    {
        private static IEnumerable<PortableExecutableReference> _staticRefs;
        public static async Task<dynamic> GetCodeCompletion(SourceInfo sourceInfo)
        {
            var refs = CompileResources.PortableExecutableCompletionReferences;

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

            string scriptCode = sourceInfo.SourceCode;
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
            int position = sourceInfo.LineNumberOffsetFromTemplate;
            var completionService = CompletionService.GetService(scriptDocument);
            var results = await completionService.GetCompletionsAsync(scriptDocument, position);
            if (results == null && sourceInfo.LineNumberOffsetFromTemplate < sourceInfo.SourceCode.Length)
            {
                sourceInfo.LineNumberOffsetFromTemplate++;
                await GetCodeCompletion(sourceInfo);
            }

            if (sourceInfo.SourceCode[sourceInfo.LineNumberOffsetFromTemplate - 1].ToString() == "(")
            {
                sourceInfo.LineNumberOffsetFromTemplate--;
                results = completionService.GetCompletionsAsync(scriptDocument, sourceInfo.LineNumberOffsetFromTemplate).Result;
            }

            //Method parameters
            var overloads = GetMethodOverloads(scriptCode, position);
            if (!(overloads?.Count > 0)) return results;
            var builder = ImmutableArray.CreateBuilder<CompletionItem>();
            foreach (var ci in overloads.Select(item => new { item, DisplayText = item })
                .Select(@t => @t.item.Split('(')[1].Split(')')[0])
                .Select(insertText => CompletionItem.Create(insertText, insertText, insertText)))
            {
                builder.Add(ci);
            }

            if (builder.Count <= 0) return results;
            var itemlist = builder.ToImmutable();
            return CompletionList.Create(new TextSpan(), itemlist);
           
        }
        public static List<string> GetMethodOverloads(string scriptCode, int position)
        {
            //position = position - 2;
            var overloads = new List<string>();
            var meta = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            Console.WriteLine($"Trusted Platform Assemblies: {meta}");
            string[] assembliesNames = meta.ToString().Split(';');
            var sourceLanguage = new CSharpLanguage();
            var syntaxTree = sourceLanguage.ParseText(scriptCode, SourceCodeKind.Script);
            var compilation = CSharpCompilation.Create("MyCompilation",
                new[] { syntaxTree }, CompileResources.PortableExecutableCompletionReferences);

            var model = compilation.GetSemanticModel(syntaxTree);

            var theToken = syntaxTree.GetRoot().FindToken(position);
            var theNode = theToken.Parent;
            while (!theNode.IsKind(SyntaxKind.InvocationExpression))
            {
                theNode = theNode.Parent;
                if (theNode == null) break; // There isn't an InvocationExpression in this branch of the tree
            }

            if (theNode != null)
            {
                var symbolInfo = model.GetSymbolInfo(theNode);

                if (symbolInfo.CandidateSymbols.Length > 0)
                {
                    overloads.AddRange(symbolInfo.CandidateSymbols
                        .Select(param => new { parameters = param, i = param.ToMinimalDisplayParts(model, position) })
                        .Where(@t => @t.parameters.Kind == SymbolKind.Method)
                        .Select(@t => @t.parameters.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                }
            }
            else
            {
                overloads = null;
            }

            return overloads;
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
}
