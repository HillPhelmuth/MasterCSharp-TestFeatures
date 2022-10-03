using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Metadata;
using MasterCsharpHosted.Client.Pages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Console = MasterCsharpHosted.Client.Pages.Console;
using LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;

namespace MasterCsharpHosted.Server.Services
{
    public class DecompileService
    {
        //private readonly CompilerService _compilerService = new();
        public static Task<string> CompileAndDecompileCode(string code, IEnumerable<MetadataReference> references)
        {
            System.Console.WriteLine("Start CompileAndDecompileCode");
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var members = root.Members;

            bool hasGlobalStatement = members.OfType<GlobalStatementSyntax>().Any();
            bool hasEntryPoint = members.OfType<ClassDeclarationSyntax>().HasEntryMethod();
            bool hasNamespace = members.Any(x => x.Kind() == SyntaxKind.NamespaceDeclaration);
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary;
            MemoryStream assembly;
            var success = false;
            if (hasNamespace || !hasGlobalStatement && hasEntryPoint)
            {
                (success, assembly) = TryCompileConsole(code, references);
            }
            else
            {
                (success, assembly) = TryCompileScript(code, references);
            }
            var sb = new StringBuilder();
            
            if (!success) return Task.FromResult("Compile failed");
            File.WriteAllBytes("TempFile.dll", assembly.ToArray());
            using (Stream assemmblySream = File.OpenRead("TempFile.dll"))
            {
                var result = DecompileAsString(assemmblySream);
                sb.AppendLine(result);
            }
            if (!File.Exists("TempFile.dll")) return Task.FromResult(sb.ToString());
            try
            {
                File.Delete("TempFile.dll");
            }
            catch(Exception ex) 
            {
                System.Console.WriteLine($"ERROR {ex.Message}\r\n{ex.StackTrace}");
            }
            return Task.FromResult(sb.ToString());
        }

        public static Task<string> DecompileAssemblyStreamAsync(Stream assemblyStream)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--------------Decompiled by MasterCSharp------------------------");
            sb.AppendLine(DecompileAsString(assemblyStream));
            return Task.FromResult(sb.ToString());
        }
        private static string DecompileAsString(Stream assemmblySream)
        {
            var peFile = new PEFile("TempFile.dll", assemmblySream);
            var assemblyResolver = new UniversalAssemblyResolver(peFile.FileName, false, peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(), PEStreamOptions.Default, MetadataReaderOptions.None);
            var decompile = new CSharpDecompiler(peFile, assemblyResolver, new DecompilerSettings());
            var result = decompile.DecompileWholeModuleAsString();
            var modules = decompile.DecompileWholeModuleAsSingleFile();
            var treeItem = modules.DescendantsAndSelf.Select(x => x.ToString());

            System.Console.WriteLine($"Decompiled Code:\r\n{result}");
            return string.Join(" ", treeItem);
        }

        //public static Task<string> DecompileAssembly(Stream assemblyStream)
        //{

        //}
        private static (bool success, MemoryStream asm) TryCompileConsole(string source, IEnumerable<MetadataReference> references)
        {
            var compilation = CSharpCompilation.Create("DynamicCode")
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication, usings: new[]
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
                }))
                .AddReferences(references)
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)));

            ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            bool error = false;
            foreach (Diagnostic diag in diagnostics)
            {
                switch (diag.Severity)
                {
                    case DiagnosticSeverity.Info:
                    case DiagnosticSeverity.Warning:
                        System.Console.WriteLine(diag.ToString());
                        break;
                    case DiagnosticSeverity.Error:
                        error = true;
                        System.Console.WriteLine(diag.ToString());
                        break;
                }
            }

            if (error)
            {
                return (false, null);
            }

            using var outputAssembly = new MemoryStream();
            compilation.Emit(outputAssembly);
            return (true, outputAssembly);
        }
        private static (bool success, MemoryStream script) TryCompileScript(string source, IEnumerable<MetadataReference> references)
        {
            var scriptCompilation = CSharpCompilation.CreateScriptCompilation(
                Path.GetRandomFileName(),
                CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.Preview)),
                references,
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
                })
            );
            var semantic = scriptCompilation.GetSemanticModel(scriptCompilation.SyntaxTrees[0]);
            var errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);
            if (errorDiagnostics.Any())
            {
                return (false, null);
            }

            using var outputAssembly = new MemoryStream();
            var emitResult = scriptCompilation.Emit(outputAssembly);

            if (!emitResult.Success) return (false, null);
            
            var script = Assembly.Load(outputAssembly.ToArray());
            return (true, outputAssembly);

        }
    }
}
