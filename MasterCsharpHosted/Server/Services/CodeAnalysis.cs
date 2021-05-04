using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private int maxLevel = 2;
        private int _currentColumn;
        private Dictionary<int, int> _rowColumns = new();
        public SyntaxTreeInfo Analyze(string programText)
        {
            for (int i = 0; i < 100; i++)
            {
                _rowColumns[i] = 0;
            }
            var tree = CSharpSyntaxTree.ParseText(programText);
            var root = tree.GetCompilationUnitRoot();

            var treeInfo = new SyntaxTreeInfo
            {
                SourceCode = programText,
                Usings = root.Usings.Select(x => x.Name.ToString()).ToList(),
                NameSpaces = root.Members.OfType<NamespaceDeclarationSyntax>().Select(syntax =>
                    WriteNamespaceInfo(syntax)).ToList(),
                Classes = root.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax))
                    .ToList(),
                Methods = root.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax))
                    .ToList(),
                GlobalDeclarations = root.Members.OfType<GlobalStatementSyntax>()
                    .Select(syntax => WriteStatementInfo(syntax.Statement, maxLevel)).ToList()
            };
            return treeInfo;
        }

        private GlobalDeclarationInfo WriteGlobalInfo(GlobalStatementSyntax globalStatement, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
            return new GlobalDeclarationInfo
            {
                Name = "Declaration",
                RootLevel = rootLevel,
                Column = column,
                Type = globalStatement.Statement.Kind().ToString(),
                RawCode = globalStatement.ToFullString()
            };
        }
        private NameSpaceInfo WriteNamespaceInfo(NamespaceDeclarationSyntax nameSpace, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            maxLevel = rootLevel > maxLevel ? rootLevel : maxLevel;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
           
            return new NameSpaceInfo
            {
                Name = nameSpace.Name.ToFullString(),
                RawCode = nameSpace.ToFullString(),
                Classes = nameSpace.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, rootLevel + 1)).ToList(),
                Column = column
            };
        }

        private ClassInfo WriteClassInfo(ClassDeclarationSyntax cls, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            int root = rootLevel + 1;
            maxLevel = rootLevel > maxLevel ? rootLevel : maxLevel;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
           
            return new ClassInfo
            {
                Name = cls.Identifier.ToString(),
                ParentName = cls.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                RawCode = cls.ToFullString(),
                RootLevel = rootLevel,
                Column = column,
                Methods = cls.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax, root+1)).ToList(),
                NestedClasses = cls.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, root+1)).ToList(),
                Properties = cls.Members.OfType<PropertyDeclarationSyntax>().Select(syntax => WritePropertyInfo(syntax, root)).ToList(),
                Fields = cls.Members.OfType<FieldDeclarationSyntax>().Select(syntax => WriteFieldInfo(syntax, root)).ToList()
            };
        }

        private MethodInfo WriteMethodInfo(MethodDeclarationSyntax method, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            maxLevel = rootLevel > maxLevel ? rootLevel : maxLevel;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
            return new MethodInfo
            {
                ParentName = method.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                Name = method.Identifier.Text,
                ReturnType = method.ReturnType.ToString(),
                RawCode = method.ToFullString(),
                Column = column,
                Parameters = method.ParameterList.Parameters.ToDictionary(x => x.Identifier.Text, x => x.Type?.ToString()),
                Body = method.Body?.ToFullString(),
                BodySyntax = method.Body?.Statements.Select(s => WriteStatementInfo(s, rootLevel + 1)).ToList(),
                RootLevel = rootLevel
            };
        }
       private GlobalDeclarationInfo WriteStatementInfo(StatementSyntax statement, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            var kind = statement.Kind();
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
            
            string type = statement switch
            {
                LocalDeclarationStatementSyntax variable => variable.Declaration.Type.ToFullString(),
                LocalFunctionStatementSyntax func => func.ReturnType.ToFullString(),
                _ => ""
            };
            return new GlobalDeclarationInfo
            {
                Name = kind == SyntaxKind.LocalDeclarationStatement ? "Variable" : kind.ToSplitCamelCase(),
                RawCode = statement.ToFullString(),
                Type = type,
                RootLevel = rootLevel,
                Column = column
            };
        }
        private PropertyInfo WritePropertyInfo(PropertyDeclarationSyntax property, int rootLevel)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            maxLevel = rootLevel > maxLevel ? rootLevel : maxLevel;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
           
            return new PropertyInfo
            {
                Name = property.Identifier.Text,
                ParentName = property.Parent?.GetText().Lines[0].ToString().TrimStart(),
                Type = property.Type.ToString(),
                RawCode = property.ToFullString(),
                Column = column,
                RootLevel = rootLevel
            };
        }

        private PropertyInfo WriteFieldInfo(FieldDeclarationSyntax field, int rootLevel = 1)
        {
            int modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            maxLevel = rootLevel > maxLevel ? rootLevel : maxLevel;
            int column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
            
            return new PropertyInfo
            {
                Name = field.Declaration.Variables[0].Identifier.Text,
                ParentName = field.Parent?.GetText().Lines[0].ToString().TrimStart(),
                Type = field.Declaration.Type.ToString(),
                RawCode = field.ToFullString(),
                Column = column,
                RootLevel = rootLevel
            };
        }
    }

    public static class SytaxExtensions
    {
        public static string ToSplitCamelCase(this Enum kind)
        {
            string input = kind.ToString();
            return Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();

        }
    }
}
