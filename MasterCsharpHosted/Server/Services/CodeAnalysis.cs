using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace MasterCsharpHosted.Server.Services
{
    public class CodeAnalysis
    {
        private int _maxLevel = 2;
        private readonly Dictionary<int, int> _rowColumns = new();
        private readonly CompileResources _compileResources;
        private SemanticModel _semanticModel;
        public CodeAnalysis(CompileResources compileResources)
        {
            _compileResources = compileResources;
        }
        public List<FullSyntaxTree> AnalyzeSimpleTree(string programText)
        {
            var tree = CSharpSyntaxTree.ParseText(programText);
            var compilation = CSharpCompilation.Create(Path.GetRandomFileName())
                .AddReferences(_compileResources.PortableExecutableReferences).AddSyntaxTrees(tree);
            _semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetCompilationUnitRoot();
            var result = new List<FullSyntaxTree>();
            foreach (var member in root.Members)
            {
                var node = member.Kind();
                var simple = new FullSyntaxTree
                {
                    Kind = Enum.GetName(node),
                    RawCode = member.ToFullString(),
                    Name = member.ToDeclarationIdentifier(),
                    Type = _semanticModel.GetTypeInfo(member).Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? 
                           _semanticModel.GetTypeInfo(member).ConvertedType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                };
                GetChildMembers(member, simple);
                result.Add(simple);
            }
            return result;
        }

        private void GetChildMembers(SyntaxNode member, FullSyntaxTree full)
        {
            //var subResult = new List<FullSyntaxTree>();
            foreach (var child in member.ChildNodes())
            {
                var kind = Enum.GetName(child.Kind());
                var code = child.ToFullString();
                var subFullTree = new FullSyntaxTree
                {
                    Kind = kind,
                    RawCode = code,
                    Name = child.ToIdentifier(),
                    Type = _semanticModel.GetTypeInfo(child).Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? 
                           _semanticModel.GetTypeInfo(child).ConvertedType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                };
                //subResult.Add(subSimple);
                full.Members.Add(subFullTree);
                if (child.ChildNodes().Any())
                {
                    GetChildMembers(child, subFullTree);
                }
            }
        }

        public SyntaxTreeInfo Analyze(string programText)
        {
            for (var i = 0; i < 100; i++)
            {
                _rowColumns[i] = 0;
            }
            var tree = CSharpSyntaxTree.ParseText(programText);
            var root = tree.GetCompilationUnitRoot();
            var fileScopedNamespaces = root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().Select(syntax =>
                WriteNamespaceInfo(syntax)).ToList();
            var treeInfo = new SyntaxTreeInfo
            {
                SourceCode = programText,
                Usings = root.Usings.Select(x => x.Name.ToString()).ToList(),
                NameSpaces = root.Members.OfType<NamespaceDeclarationSyntax>().Select(syntax =>
                    WriteNamespaceInfo(syntax)).Concat(fileScopedNamespaces).ToList(),
                Classes = root.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax))
                    .ToList(),
                Methods = root.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax))
                    .ToList(),
                GlobalDeclarations = root.Members.OfType<GlobalStatementSyntax>()
                    .Select(syntax => WriteStatementInfo(syntax.Statement, _maxLevel)).ToList()
            };
            Console.WriteLine(JsonConvert.SerializeObject(treeInfo, Formatting.Indented));
            return treeInfo;
        }

        #region Convert Roslyn to simplified syntax
        private NameSpaceInfo WriteNamespaceInfo(BaseNamespaceDeclarationSyntax nameSpace, int rootLevel = 1)
        {
            var modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            _maxLevel = rootLevel > _maxLevel ? rootLevel : _maxLevel;
            var column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;

            var namespaceInfo = new NameSpaceInfo
            {
                Name = nameSpace.Name.ToFullString(),
                RawCode = nameSpace.ToFullString(),
                //Classes = nameSpace.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, rootLevel + 1)).ToList(),
                Column = column
            };
            namespaceInfo.AddClasses(nameSpace.Members.OfType<ClassDeclarationSyntax>().Select(syntax => WriteClassInfo(syntax, rootLevel++)).ToList());
            namespaceInfo.AddEnums(nameSpace.Members.OfType<EnumDeclarationSyntax>().Select(x => WriteEnumInfo(x, rootLevel + 1)).ToList());
            return namespaceInfo;
        }

        private ClassInfo WriteClassInfo(TypeDeclarationSyntax cls, int rootLevel = 1)
        {
            var modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            var root = rootLevel + 1;
            _maxLevel = rootLevel > _maxLevel ? rootLevel : _maxLevel;
            var column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;

            var classinfo = new ClassInfo
            {
                Name = cls.Identifier.ToString(),
                ParentName = cls.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                RawCode = cls.ToFullString(),
                RootLevel = rootLevel,
                Column = column,
            };
            var ctors = cls.Members.ConvertToInfo<MethodInfo, ConstructorDeclarationSyntax>(syntax => WriteConstructorSyntax(syntax, root));
            classinfo.Constructors = ctors;
            //var methods = cls.Members.OfType<MethodDeclarationSyntax>().Select(syntax => WriteMethodInfo(syntax, root + 1)).ToList();
            var methods = cls.Members.ConvertToInfo<MethodInfo, MethodDeclarationSyntax>(syntax => WriteMethodInfo(syntax, root + 1));
            classinfo.AddMethods(methods);
            var nested = cls.Members.ConvertToInfo<ClassInfo, ClassDeclarationSyntax>(syntax => WriteClassInfo(syntax, root + 1));
            classinfo.AddNestedClasses(nested);
            classinfo.AddProperties(cls.Members.ConvertToInfo<PropertyInfo, PropertyDeclarationSyntax>(syntax => WritePropTypeInfo(syntax, root)));
            classinfo.AddFields(cls.Members.OfType<FieldDeclarationSyntax>().Select(syntax => WritePropTypeInfo(syntax, root)).ToList());
            var events = cls.Members.ConvertToInfo<PropertyInfo, EventDeclarationSyntax>(s => WritePropTypeInfo(s, root));
            classinfo.AddProperties(events);
            var eventFields = cls.Members.ConvertToInfo<PropertyInfo, EventFieldDeclarationSyntax>(s => WritePropTypeInfo(s, root));
            classinfo.AddProperties(eventFields);
            var enums = cls.Members.OfType<EnumDeclarationSyntax>().Select(x => WriteEnumInfo(x, root + 1)).ToList();
            classinfo.AddEnums(enums);
            return classinfo;
        }
        private EnumInfo WriteEnumInfo(EnumDeclarationSyntax enm, int rootLevel = 1)
        {
            var modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            rootLevel += modifier;
            var root = rootLevel + 1;
            _maxLevel = rootLevel > _maxLevel ? rootLevel : _maxLevel;
            var column = _rowColumns[rootLevel];
            _rowColumns[rootLevel] = _rowColumns[rootLevel] >= 5 ? 0 : _rowColumns[rootLevel] + 1;
            var enumInfo = new EnumInfo
            {
                Name = enm.Identifier.ToString(),
                //ParentName = cls.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                RawCode = enm.ToFullString(),
                RootLevel = rootLevel,
                Column = column,
            };
            enumInfo.AddFields(enm.Members.OfType<EnumMemberDeclarationSyntax>().Select(syntax => WritePropTypeInfo(syntax, root)).ToList());
            return enumInfo;
        }
        private MethodInfo WriteConstructorSyntax(ConstructorDeclarationSyntax constructor, int rootLevel = 1)
        {
            (var column, var row) = SetColumnRow(rootLevel);
            var ctorInfo = new MethodInfo
            {
                ParentName = constructor.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                Name = constructor.Identifier.Text,
                ReturnType = $"{string.Join(" ", constructor.Modifiers.Select(x => x.Text))} {constructor.Identifier.Text}",
                Parameters = constructor.ParameterList.Parameters.ToDictionary(x => x.Identifier.Text, x => x.Type?.ToString()),
                Body = constructor.Body?.ToFullString(),
                RawCode = constructor.ToFullString(),
                Column = column,
                RootLevel = row
            };
            var ctorMembers = constructor.Body.Statements.Select(s => WriteStatementInfo(s, row + 1)).ToList();
            ctorInfo.AddMethodMemebers(ctorMembers);
            return ctorInfo;

        }
        private MethodInfo WriteMethodInfo(MethodDeclarationSyntax method, int rootLevel = 1)
        {
            (var column, var row) = SetColumnRow(rootLevel);
            var methodInfo = new MethodInfo
            {
                ParentName = method.Parent?.GetText().Lines[0].ToString().TrimStart() ?? "No ParentName",
                Name = method.Identifier.Text,
                ReturnType = method.ReturnType.ToString(),
                RawCode = method.ToFullString(),
                Column = column,
                Parameters = method.ParameterList.Parameters.ToDictionary(x => x.Identifier.Text, x => x.Type?.ToString()),
                Body = method.Body?.ToFullString(),
                //BodySyntax = method.Body?.Statements.Select(s => WriteStatementInfo(s, rootLevel + 1)).ToList(),
                RootLevel = row
            };
            var members = method.Body?.Statements.Select(s => WriteStatementInfo(s, row + 1)).ToList();
            methodInfo.AddMethodMemebers(members);
            return methodInfo;
        }

        private GlobalDeclarationInfo WriteStatementInfo(StatementSyntax statement, int rootLevel = 1)
        {
            (var column, var row) = SetColumnRow(rootLevel);
            var kind = statement.Kind();
            var type = statement switch
            {
                LocalDeclarationStatementSyntax variable => variable.Declaration.Type.ToFullString(),
                LocalFunctionStatementSyntax func => func.ReturnType.ToFullString(),
                BlockSyntax block => $"Block statements:\r\n{string.Join(", ", block.Statements.Select(s => s.GetType().Name))}",
                ForEachStatementSyntax forEach => forEach.Type.ToFullString(),
                ForStatementSyntax forStatement => $"{string.Join("\r\n", forStatement.Statement.ChildTokens().SelectMany(x => x.ToFullString()))}",
                _ => ""
            };
            return new GlobalDeclarationInfo
            {
                Name = kind == SyntaxKind.LocalDeclarationStatement ? "Variable" : kind.ToSplitCamelCase(),
                RawCode = statement.ToFullString(),
                Type = type,
                RootLevel = row,
                Column = column
            };
        }
        private PropertyInfo WritePropTypeInfo<T>(T member, int rootLevel) where T : MemberDeclarationSyntax
        {
            (var column, var row) = SetColumnRow(rootLevel);
            return member switch
            {
                EventDeclarationSyntax eventSyntax => new PropertyInfo
                {
                    Name = eventSyntax.Identifier.Text,
                    ParentName = eventSyntax.Parent?.GetText().Lines[0].ToString().TrimStart(),
                    Type = eventSyntax.Type.ToString(),
                    RawCode = eventSyntax.ToFullString(),
                    Column = column,
                    RootLevel = row
                },
                PropertyDeclarationSyntax property => new PropertyInfo
                {
                    Name = property.Identifier.Text,
                    ParentName = property.Parent?.GetText().Lines[0].ToString().TrimStart(),
                    Type = property.Type.ToString(),
                    RawCode = property.ToFullString(),
                    Column = column,
                    RootLevel = row
                },
                FieldDeclarationSyntax field => new PropertyInfo
                {
                    Name = field.Declaration.Variables[0].Identifier.Text,
                    ParentName = field.Parent?.GetText().Lines[0].ToString().TrimStart(),
                    Type = field.Declaration.Type.ToString(),
                    RawCode = field.ToFullString(),
                    Column = column,
                    RootLevel = row
                },
                EventFieldDeclarationSyntax eventField => new PropertyInfo
                {
                    Name = eventField.Declaration.Variables[0].Identifier.Text,
                    ParentName = eventField.Parent?.GetText().Lines[0].ToString().TrimStart(),
                    Type = eventField.Declaration.Type.ToString(),
                    RawCode = eventField.ToFullString(),
                    Column = column,
                    RootLevel = row
                },
                EnumMemberDeclarationSyntax enumMember => new PropertyInfo
                {
                    Name = enumMember.Identifier.Text,
                    ParentName = enumMember.Parent?.GetText().Lines[0].ToString().TrimStart(),
                    Type = "enum value",
                    RawCode = enumMember.ToFullString(),
                    Column = column,
                    RootLevel = row
                },
                _ => new PropertyInfo()
            };

        }
        
        #endregion

        private (int column, int row) SetColumnRow(int rootLevel)
        {
            var modifier = _rowColumns[rootLevel] >= 5 ? 1 : 0;
            var row = rootLevel + modifier;
            _maxLevel = row > _maxLevel ? row : _maxLevel;
            var column = _rowColumns[row];
            _rowColumns[row] = _rowColumns[row] >= 5 ? 0 : _rowColumns[row] + 1;
            return (column, row);
        }

    }

    public static class SytaxExtensions
    {
        public static List<TValue> ConvertToInfo<TValue, TItem>(this SyntaxList<MemberDeclarationSyntax> items, Func<TItem, TValue> convertDelegate) where TItem : MemberDeclarationSyntax
                                                                                                                                                       where TValue : SyntaxInfoBase
        {
            return items.OfType<TItem>().Select(convertDelegate).ToList();
        }
        public static string ToDeclarationIdentifier<T>(this T declaration) where T : CSharpSyntaxNode
        {
            return declaration switch
            {
                EventDeclarationSyntax eventSyntax => $"{eventSyntax.Type.ToFullString()} {eventSyntax.Identifier.Text}",
                PropertyDeclarationSyntax propSyntax => $"{propSyntax.Type.ToFullString()} {propSyntax.Identifier.Text}",
                EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables[0].Identifier.Text,
                FieldDeclarationSyntax field => $"{field.Declaration.Type} {field.Declaration.Variables[0].Identifier.Text}",
                MethodDeclarationSyntax method => $"{method.ReturnType.ToFullString()} {method.Identifier.Text}",
                ClassDeclarationSyntax cls => cls.Identifier.Text,
                EnumDeclarationSyntax enm => enm.Identifier.Text,
                EnumMemberDeclarationSyntax enumMem => enumMem.Identifier.Text,
                NamespaceDeclarationSyntax name => name.Name.ToFullString(),
                VariableDeclaratorSyntax variable => $"{variable.Identifier.Text}",
                VariableDeclarationSyntax varDec => $"{varDec.Type.ToFullString()} {varDec.Variables[0].Identifier.Text}",
                LocalDeclarationStatementSyntax local => $"{local.Declaration.Type.ToFullString()} {local.Declaration.Variables[0].Identifier.Text}",
                LocalFunctionStatementSyntax localFunc => $"{localFunc.ReturnType.ToFullString()} {localFunc.Identifier.Text}",
                ParameterSyntax parameter => $"{parameter.Type?.ToFullString()} {parameter.Identifier.Text}",
                MemberAccessExpressionSyntax ass => $"{ass.Name.Identifier.Text} ({ass.Name.Kind()} {ass.Name.Identifier.ValueText})",
                _ => ""
            };
        }
        public static string ToIdentifier<T>(this T node) where T : SyntaxNode
        {
            var result = "";
            if (node is CSharpSyntaxNode memberDeclaration) 
                result = memberDeclaration.ToDeclarationIdentifier();
            else
            {

                Console.WriteLine($"Type of Child Member is not {nameof(CSharpSyntaxNode)}/r/n It is of Type -- {node.GetType()}");
            }
            
            return result;
        }
    }
}
