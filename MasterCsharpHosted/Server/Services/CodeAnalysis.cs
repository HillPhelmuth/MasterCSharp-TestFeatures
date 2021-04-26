using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MasterCsharpHosted.Server.Services
{
    public class CodeAnalysis
    {
        public SyntaxTreeInfo Analyze(string programText)
        {

            var tree = CSharpSyntaxTree.ParseText(programText);
            var root = tree.GetCompilationUnitRoot();
           
            return new SyntaxTreeInfo
            {
                SourceCode = programText,
                Usings = root.Usings.Select(x => x.Name.ToString()).ToList(),
                NameSpaces = root.Members.OfType<NamespaceDeclarationSyntax>().Select(syntax =>
                    WriteNamespaceInfo(syntax)).ToList(),
                Classes = root.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax)).ToList(),
                Methods = root.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax)).ToList(),
                GlobalDeclarations = root.Members.OfType<GlobalStatementSyntax>().Select(WriteGlobalDeclarationInfo).ToList()
            };

        }

        private static GlobalDeclarationInfo WriteGlobalDeclarationInfo(GlobalStatementSyntax globalStatement)
        {
            
            return new()
            {
                Name = "Declaration",
                RootLevel = 2,
                Type = globalStatement.Statement.Kind().ToString(),
                RawCode = globalStatement.ToFullString()
            };
        }
        private static NameSpaceInfo WriteNamespaceInfo(NamespaceDeclarationSyntax nameSpace, int rootLevel = 1)
        {
            return new()
            {
                Name = nameSpace.Name.ToFullString(),
                RawCode = nameSpace.ToFullString(),
                Classes = nameSpace.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, rootLevel + 1)).ToList()
            };
        }

        private static ClassInfo WriteClassInfo(ClassDeclarationSyntax cls, int rootLevel = 1)
        {
            int root = rootLevel + 1;
            return new()
            {
                Name = cls.Identifier.ToString(),
                ParentName = cls.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                RawCode = cls.ToFullString(),
                RootLevel = rootLevel,
                Methods = cls.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax, root)).ToList(),
                NestedClasses = cls.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, root)).ToList(),
                Properties = cls.Members.OfType<PropertyDeclarationSyntax>().Select(syntax => WritePropertyInfo(syntax, root + 1)).ToList(),
                Fields = cls.Members.OfType<FieldDeclarationSyntax>().Select(syntax => WriteFieldInfo(syntax, root + 1)).ToList()
            };
        }

        private static MethodInfo WriteMethodInfo(MethodDeclarationSyntax method, int rootLevel = 1)
        {
            return new()
            {
                ParentName = method.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                Name = method.Identifier.Text,
                ReturnType = method.ReturnType.ToString(),
                RawCode = method.ToFullString(),
                Parameters = method.ParameterList.Parameters.ToDictionary(x => x.Identifier.Text, x => x.Type?.ToString()),
                Body = method.Body?.ToFullString(),
                RootLevel = rootLevel
            };
        }

        private static PropertyInfo WritePropertyInfo(PropertyDeclarationSyntax property, int rootLevel)
        {
            return new()
            {
                Name = property.Identifier.Text,
                ParentName = property.Parent?.GetText().Lines[0].ToString().TrimStart(),
                Type = property.Type.ToString(),
                RawCode = property.ToFullString(),
                RootLevel = rootLevel
            };
        }

        private static PropertyInfo WriteFieldInfo(FieldDeclarationSyntax field, int rootLevel = 1)
        {
            return new()
            {
                Name = field.Declaration.Variables[0].Identifier.Text,
                ParentName = field.Parent?.GetText().Lines[0].ToString().TrimStart(),
                Type = field.Declaration.Type.ToString(),
                RawCode = field.ToFullString(),
                RootLevel = rootLevel
            };
        }
    }
}
