using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MasterCsharpHosted.Server.Services
{
    public partial class CompilerService
    {
        private CSharpCompilation runningCompilation;
        private IEnumerable<MetadataReference> _references;
        private object[] submissionStates = { null, null };
        private int submissionIndex = 0;
        
        public string CodeOutput { get; set; }
        
        public async Task<bool> SubmitSolution(string code, IEnumerable<MetadataReference> references, string testAgainst = "true")
        {
            Console.WriteLine("Compiling and running code");
            var sw = Stopwatch.StartNew();
            await RunSubmission(code, references);
            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms elapsed \n output: {CodeOutput}");
            return CodeOutput == testAgainst || CodeOutput == $"\"{testAgainst}\"";
        }

        public async Task<string> SubmitCode(string code, IEnumerable<MetadataReference> references)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var members = root.Members;
            if (members.Any(x => x.Kind() == SyntaxKind.NamespaceDeclaration))
            {
                CodeOutput = await RunConsole(code, references);
                Console.WriteLine($"Code output: {CodeOutput}");
                return CodeOutput;
            }
            if (!members.OfType<GlobalStatementSyntax>().Any())
            {
                if (members.OfType<ClassDeclarationSyntax>().IsClassWithMainMethod())
                {
                    CodeOutput = await RunConsole(code, references);
                    Console.WriteLine($"Code output: {CodeOutput}");
                    return CodeOutput;
                }
            }
            await RunSubmission(code, references);
            Console.WriteLine($"Code output: {CodeOutput}");
            return CodeOutput;
        }
      
        public async Task RunSubmission(string code, IEnumerable<MetadataReference> references)
        {
            _references = references;
           
            var previousOut = Console.Out;
            try
            {
                if (TryCompile(code, out var script, out var errorDiagnostics))
                {
                    var writer = new StringWriter();
                    Console.SetOut(writer);

                    var entryPoint = runningCompilation.GetEntryPoint(CancellationToken.None);
                    var type = script.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
                    var entryPointMethod = type.GetMethod(entryPoint.MetadataName);

                    var submission = (Func<object[], Task>)entryPointMethod.CreateDelegate(typeof(Func<object[], Task>));

                    if (submissionIndex >= submissionStates.Length)
                    {
                        Array.Resize(ref submissionStates, Math.Max(submissionIndex, submissionStates.Length * 2));
                    }

                    var returnValue = await (Task<object>)submission(submissionStates);
                    if (returnValue != null)
                    {
                        Console.Write(CSharpObjectFormatter.Instance.FormatObject(returnValue));
                    }

                    CodeOutput = writer.ToString();
                    string output = HttpUtility.HtmlEncode(writer.ToString());
                }
                else
                {
                    string errorOutput = errorDiagnostics.Aggregate("", (current, diag) => current + HttpUtility.HtmlEncode(diag));

                    CodeOutput = $"COMPILE ERROR: {errorOutput}";
                    
                }
            }
            catch (Exception ex)
            {
                CodeOutput = $"EXECUTION FAILED: {HttpUtility.HtmlEncode(CSharpObjectFormatter.Instance.FormatException(ex))}";
            }
            finally
            {
                Console.SetOut(previousOut);
            }
        }

        //Tries to compile, if successful, it outputs the DLL Assembly. If unsuccessful, it will output the error message
        private bool TryCompile(string source, out Assembly assembly, out IEnumerable<Diagnostic> errorDiagnostics)
        {
            assembly = null;
            var scriptCompilation = CSharpCompilation.CreateScriptCompilation(
                Path.GetRandomFileName(),
                CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.Preview)),
                _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[]
                {
                    "System",
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
                    "System.Numerics",
                    "Microsoft.CodeAnalysis",
                    "Microsoft.CodeAnalysis.CSharp"
                }),
                runningCompilation
            );

            errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);
            if (errorDiagnostics.Any())
            {
                return false;
            }

            using var peStream = new MemoryStream();
            var emitResult = scriptCompilation.Emit(peStream);

            if (!emitResult.Success) return false;
            submissionIndex++;
            runningCompilation = scriptCompilation;
            assembly = Assembly.Load(peStream.ToArray());
            return true;

        }
    }
    public static class CompileExtensions
    {
        public static bool IsClassWithMainMethod(this IEnumerable<ClassDeclarationSyntax> memberClasses)
        {
            return memberClasses.Any(cls => cls.Members.OfType<MethodDeclarationSyntax>().Any(mthd => mthd.Identifier.Text == "Main"));
        }
    }
}
