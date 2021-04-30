using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Components;
using Blazor.Diagrams.Core.Geometry;
using Microsoft.AspNetCore.Components.Web;

namespace CodeSyntaxModule
{
    public partial class CodeDiagram : IDisposable
    {
        [Inject]
        public AppState AppState { get; set; }
        [Parameter]
        public SyntaxTreeInfo SyntaxTreeInfo { get; set; }
        [Parameter]
        public EventCallback<string> SendCode { get; set; }

        public CSharpDiagramState DiagramState { get; set; } = new();
        
        private List<SyntaxNode> _namespaceNodes = new();
        private List<SyntaxNode> _classNodes = new();
        private List<SyntaxNode> _methodNodes = new();
        private List<SyntaxNode> _propNodes = new();
        private List<SyntaxNode> _fieldNodes = new();
        private List<SyntaxNode> _globalNodes = new();
        private Dictionary<string, List<SyntaxNode>> _methodMemberNodes = new();
        private List<LinkModel> _nodeLinks = new();
        
        private static DiagramOptions _options = new()
        {
            DeleteKey = "None",
            AllowMultiSelection = true,
            AllowPanning = true,
            Zoom = new DiagramZoomOptions
            {
                Inverse = false
            },
            Links = new DiagramLinkOptions
            {
                DefaultColor = "blue",
                DefaultSelectedColor = "red"
            }
        };
        private List<SyntaxGroup> _groups = new();
        private Diagram _diagram = new(_options);
        private Dictionary<int, int> _rowColumns = new();
        private bool _isExpandMethods;
        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += AppStateChanged;
            for (int i = 0; i < 100; i++) _rowColumns[i] = 0;

            
            RegisterEvents();
            _diagram.RegisterModelComponent<SyntaxNode, CSharpNode>();
            _diagram.RegisterModelComponent<SyntaxGroup, SyntaxGroupWidget>();
            int nsIndex = 1;
            foreach (var nameSpace in SyntaxTreeInfo.NameSpaces)
            {
                var node = new SyntaxNode(new Point((150 * _rowColumns[nameSpace.RootLevel]) + 175, 0))
                {
                    Name = nameSpace.Name,
                    RawCode = nameSpace.RawCode,
                    NodeKind = NodeKind.Namespace
                };
                _rowColumns[nameSpace.RootLevel]++;
                node.AddPort();
                _namespaceNodes.Add(node);
                foreach (var cls in nameSpace.Classes)
                {
                    var classNode = GetClassSyntaxNode(cls, node);
                    var classMembers = CreateClassMemberNodes(cls, classNode);
                    var syntaxGroup = new SyntaxGroup(classMembers, cls.Name, NodeKind.Class, 50);
                    syntaxGroup.AddNodePorts();
                    _groups.Add(syntaxGroup);
                    _nodeLinks.Add(new LinkModel(classNode.GetPort(PortAlignment.Bottom), syntaxGroup.GetPort(PortAlignment.Top)));
                }

                nsIndex++;
            }

            foreach (var cls in SyntaxTreeInfo.Classes)
            {
                var classNode = GetClassSyntaxNode(cls);
                var classGroup = CreateClassMemberNodes(cls, classNode);
                var syntaxGroup = new SyntaxGroup(classGroup, cls.Name, NodeKind.Class, 50);
                syntaxGroup.AddNodePorts();
                _groups.Add(syntaxGroup);
                _nodeLinks.Add(new LinkModel(classNode.GetPort(PortAlignment.Bottom), syntaxGroup.GetPort(PortAlignment.Top)));
            }

            if (SyntaxTreeInfo.GlobalDeclarations.Count > 0)
            {
                SyntaxNode previousNode = null;
                var globalNode = new SyntaxNode(new Point(200 * _rowColumns[1], 0))
                {
                    Name = "Syntax Tree",
                    RawCode = SyntaxTreeInfo.SourceCode,
                    NodeKind = NodeKind.None
                };
                globalNode.AddNodePorts();
                
                foreach (var global in SyntaxTreeInfo.GlobalDeclarations.Select(g => CreateGlobalNode(g)))
                {
                    _globalNodes.Add(global);
                    if (previousNode != null)
                    {
                        _nodeLinks.Add(new LinkModel(previousNode.GetPort(PortAlignment.Right), global.GetPort(PortAlignment.Left)));
                    }
                    _nodeLinks.Add(new LinkModel(globalNode.GetPort(PortAlignment.Bottom), global.GetPort(PortAlignment.Top)));
                    previousNode = global;
                }
                _diagram.Nodes.Add(globalNode);
            }
            if (SyntaxTreeInfo.Methods.Count > 0)
            {
                _methodNodes.AddRange(SyntaxTreeInfo.Methods.Select(CreateMethodNode));
                _diagram.Nodes.Add(_methodNodes);
            }
            _diagram.Nodes.Add(_namespaceNodes);
            _diagram.Nodes.Add(_classNodes);
            _diagram.Nodes.Add(_methodNodes.Union(_propNodes).Union(_fieldNodes).Union(_globalNodes));
            _diagram.Links.Add(_nodeLinks);
            _groups.ForEach(g => _diagram.AddGroup(g));

            return base.OnInitializedAsync();
        }

        private void ZoomToFit(MouseEventArgs args)
        {
            _diagram.ZoomToFit(50);
        }
        private void RegisterEvents()
        {
            _diagram.SelectionChanged += (m) =>
            {
                Console.WriteLine($"SelectionChanged, Id={m.Id}, Type={m.GetType().Name}, Selected={m.Selected}");
                switch (m)
                {
                    case SyntaxNode node:
                    {
                        SendCode.InvokeAsync(node.RawCode);
                        //if (node.NodeKind == NodeKind.Method && _isExpandMethods)
                        //{
                        //    CreateMethodGroupNode(node);
                        //}

                        break;
                    }
                    case SyntaxGroup grp:
                    {
                        var n = grp.Ports.Select(p => p.Parent).FirstOrDefault();
                        string code = (n as SyntaxNode)?.RawCode;
                        if (!string.IsNullOrWhiteSpace(code))
                            SendCode.InvokeAsync(code);
                        break;
                    }
                }

                StateHasChanged();
            };
           
        }
        private List<SyntaxNode> CreateClassMemberNodes(ClassInfo cls, SyntaxNode classNode = null)
        {
            var classMembers = new List<SyntaxNode>();
            foreach (var method in cls.Methods)
            {
                var methodNode = CreateMethodNode(method);
                var methodGroup = CreateMethodMembersNodes(method);
                var syntaxGroup = new SyntaxGroup(methodGroup, methodNode.Name, NodeKind.Class, 50);
                syntaxGroup.AddNodePorts();
                _groups.Add(syntaxGroup);
                _nodeLinks.Add(new LinkModel(methodNode.GetPort(PortAlignment.Bottom), syntaxGroup.GetPort(PortAlignment.Top)));
                _methodMemberNodes[methodNode.Id] = methodGroup;
               
                classMembers.Add(methodNode);
            }

            cls.NestedClasses.ForEach(x => CreateClassMemberNodes(x, classNode));

            classMembers.AddRange(cls.Properties.Select(CreatePropNode));
            classMembers.AddRange(cls.Fields.Select(CreateFieldNode));

            return classMembers;
        }

        private void CreateMethodGroupNode(SyntaxNode methodNode)
        {
            var syntaxGroup = new SyntaxGroup(_methodMemberNodes[methodNode.Id], methodNode.Name, NodeKind.Method, 40);
            syntaxGroup.AddNodePorts();
            
            //_diagram.Nodes.Add(syntaxGroup);
            //_diagram.Links.Add(new LinkModel(methodNode.GetPort(PortAlignment.Bottom), syntaxGroup.GetPort(PortAlignment.Top)));
            //_diagram.AddGroup(syntaxGroup);
            
        }

        private SyntaxNode GetClassSyntaxNode(ClassInfo cls, NodeModel node = null)
        {
            var classNode = new SyntaxNode(new Point(220 * _rowColumns[cls.RootLevel], 220 * cls.RootLevel))
            {
                Name = cls.Name,
                RawCode = cls.RawCode,
                NodeKind = NodeKind.Class
            };

            _rowColumns[cls.RootLevel]++;
            classNode.AddNodePorts();

            _classNodes.Add(classNode);
            if (node != null)
                _nodeLinks.Add(new LinkModel(node.GetPort(PortAlignment.Bottom), classNode.GetPort(PortAlignment.Top)));
            return classNode;
        }

        private SyntaxNode CreateFieldNode(PropertyInfo field)
        {
            var fieldNode = new SyntaxNode(new Point(240 * _rowColumns[field.RootLevel + 1], 220 * field.RootLevel + 1))
            {
                Name = field.Name,
                RawCode = field.RawCode,
                Type = field.Type,
                NodeKind = NodeKind.Field
            };
            _rowColumns[field.RootLevel + 1]++;
            fieldNode.AddNodePorts();
            _fieldNodes.Add(fieldNode);
            return fieldNode;
        }

        private SyntaxNode CreatePropNode(PropertyInfo prop)
        {
            var propNode = new SyntaxNode(new Point(240 * _rowColumns[prop.RootLevel + 1], 220 * prop.RootLevel + 1))
            {
                Name = prop.Name,
                RawCode = prop.RawCode,
                Type = prop.Type,
                NodeKind = NodeKind.Property
            };
            _rowColumns[prop.RootLevel + 1]++;
            propNode.AddNodePorts();
            _propNodes.Add(propNode);
            return propNode;
        }

        private SyntaxNode CreateMethodNode(MethodInfo method)
        {
            var methodNode = new SyntaxNode(new Point(240 * _rowColumns[method.RootLevel], 220 * method.RootLevel))
            {
                Name = method.Name,
                RawCode = method.RawCode,
                Type = method.ReturnType,
                Parameters = method.Parameters,
                NodeKind = NodeKind.Method
            };
            _rowColumns[method.RootLevel]++;
            methodNode.AddNodePorts();
            _methodNodes.Add(methodNode);
            return methodNode;
        }

        private List<SyntaxNode> CreateMethodMembersNodes(MethodInfo method)
        {
            //return method.BodySyntax.Select(info => CreateGlobalNode(info, true)).ToList();
            return method.BodySyntax.Select(info => CreateGlobalNode(info)).ToList();
        }
        private SyntaxNode CreateGlobalNode(GlobalDeclarationInfo global, bool isMethodBody = false)
        {
            var node = new SyntaxNode(new Point(240 * _rowColumns[global.RootLevel], 220 * global.RootLevel))
            {
                Name = global.Name,
                RawCode = global.RawCode,
                Type = global.Type,
                NodeKind = NodeKind.Global
            };
            if (isMethodBody) return node;
            _rowColumns[global.RootLevel]++;
            node.AddNodePorts();
            _globalNodes.Add(node);
            return node;
        }

        private void AppStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AppState.SyntaxTreeInfo)) return;
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= AppStateChanged;
        }
    }
}
