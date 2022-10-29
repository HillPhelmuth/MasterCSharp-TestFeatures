using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MasterCsharpHosted.Shared;
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

        public async Task<CodeOutputModel> SubmitChallenge(CodeInputModel codeInput, IEnumerable<MetadataReference> references)
        {
            var testCode = codeInput.Solution;
            var outputs = new CodeOutputModel { Outputs = new List<Output>() };
            var index = 1;
            var swTot = new Stopwatch();
            Console.WriteLine("start all tasks");
            swTot.Start();

            foreach (var snip in codeInput.Tests)
            {
                var sw = new Stopwatch();

                var output = new Output { TestIndex = index, Test = snip };
                var code = $"{testCode}\n{snip.Append}";
                var expected = snip.TestAgainst;
                sw.Start();
                output.TestResult = await SubmitSolution(code, references, expected);
                output.Codeout = CodeOutput;
                output.Test = snip;
                sw.Stop();
                Console.WriteLine($"Complete task 2 in {sw.ElapsedMilliseconds}ms");
                index++;
                outputs.Outputs.Add(output);

            }
            swTot.Stop();
            Console.WriteLine($"Completed all tasks in {swTot.ElapsedMilliseconds}ms");
            return outputs;
        }
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
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var members = root.Members;
            
            var hasGlobalStatement = members.OfType<GlobalStatementSyntax>().Any();
            var hasEntryPoint = members.OfType<ClassDeclarationSyntax>().HasEntryMethod();
            var hasNamespace = members.Any(x => x.Kind() == SyntaxKind.NamespaceDeclaration);
            if (hasNamespace || !hasGlobalStatement && hasEntryPoint)
            {
                CodeOutput = await RunConsole(code, references);
                Console.WriteLine($"Code output: {CodeOutput}");
                return CodeOutput;
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
                var (success, script) = TryCompileScript(code, out var errorDiagnostics);
                if (success)
                {
                    await WriteScript(script);
                    //var output = HttpUtility.HtmlEncode(writer.ToString());
                }
                else
                {
                    var errorOutput = errorDiagnostics.Aggregate("", (current, diag) => current + HttpUtility.HtmlEncode(diag));

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

        private async Task WriteScript(Assembly script)
        {
            var writer = new StringWriter();
            Console.SetOut(writer);

            var entryPoint = runningCompilation.GetEntryPoint(CancellationToken.None);
            var type = script.GetType(
                $"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
            var entryPointMethod = type.GetMethod(entryPoint.MetadataName);

            var submission = (Func<object[], Task>) entryPointMethod.CreateDelegate(typeof(Func<object[], Task>));

            if (submissionIndex >= submissionStates.Length)
            {
                Array.Resize(ref submissionStates, Math.Max(submissionIndex, submissionStates.Length * 2));
            }

            var returnValue = await (Task<object>) submission(submissionStates);
            if (returnValue != null)
            {
                Console.Write(CSharpObjectFormatter.Instance.FormatObject(returnValue));
            }

            CodeOutput = writer.ToString();
        }

        //Tries to compile, if successful, it outputs the DLL Assembly. If unsuccessful, it will output the error message
        private (bool success, Assembly script) TryCompileScript(string source, out IEnumerable<Diagnostic> errorDiagnostics)
        {
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
                return (false, null);
            }

            using var outputAssembly = new MemoryStream();
            var emitResult = scriptCompilation.Emit(outputAssembly);

            if (!emitResult.Success) return (false, null);
            submissionIndex++;
            runningCompilation = scriptCompilation;
            var script = Assembly.Load(outputAssembly.ToArray());
            return (true, script);

        }
    }
    public static class CompileExtensions
    {
        public static bool HasEntryMethod(this IEnumerable<ClassDeclarationSyntax> memberClasses)
        {
            return memberClasses.Any(cls => cls.Members.OfType<MethodDeclarationSyntax>().Any(mthd => mthd.Identifier.Text == "Main"));
        }
    }
}
