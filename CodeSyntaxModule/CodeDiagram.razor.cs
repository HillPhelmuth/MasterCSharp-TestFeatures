using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Components;
using Blazor.Diagrams.Core.Geometry;

namespace CodeSyntaxModule
{
    public partial class CodeDiagram
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
        private List<LinkModel> _nodeLinks = new();
        private static DiagramOptions _options = new()
        {

            DeleteKey = "Delete",
            DefaultNodeComponent = null,
            //GridSize = 400,
            AllowMultiSelection = true,
            AllowPanning = true,
            Zoom = new DiagramZoomOptions
            {
                Inverse = false
            },
            Links = new DiagramLinkOptions
            {
                DefaultLinkComponent = null,
                DefaultColor = "blue",
                DefaultSelectedColor = "red"
            }
        };
        private List<GroupModel> _groups = new();
        private Diagram _diagram = new(_options);
        private Dictionary<int, int> _rowColumns = new();
        protected override Task OnInitializedAsync()
        {
            for (int i = 1; i < 100; i++) _rowColumns[i] = 0;

            DiagramState.OnSendRawCode += s => SendCode.InvokeAsync(s);
            RegisterEvents();
            _diagram.RegisterModelComponent<SyntaxNode, CSharpNode>();
            int nsIndex = 1;
            foreach (var nameSpace in SyntaxTreeInfo.NameSpaces)
            {
                var node = new SyntaxNode(new Point(10 * nameSpace.RootLevel, 0))
                {
                    Name = nameSpace.Name,
                    RawCode = nameSpace.RawCode,
                    NodeKind = NodeKind.Namespace
                };
                _rowColumns[nameSpace.RootLevel]++;
                node.AddPort();
                _namespaceNodes.Add(node);
                foreach (var classMembers in nameSpace.Classes.Select(cls => CreateClassMemberNodes(cls, node)))
                {
                    _groups.Add(new GroupModel(classMembers, 20));
                }

                nsIndex++;
            }

            foreach (var classGroup in SyntaxTreeInfo.Classes.Select(c => CreateClassMemberNodes(c)))
            {
                _groups.Add(new GroupModel(classGroup, 20));
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
                foreach (var global in SyntaxTreeInfo.GlobalDeclarations.Select(CreateGlobalNode))
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Task.Delay(1000);
                _diagram.ZoomToFit(50);
            }
                
            await base.OnAfterRenderAsync(firstRender);
        }

        private void RegisterEvents()
        {
            _diagram.SelectionChanged += (m) =>
            {
                Console.WriteLine($"SelectionChanged, Id={m.Id}, Type={m.GetType().Name}, Selected={m.Selected}");
                if (m is SyntaxNode node)
                {
                    SendCode.InvokeAsync(node.RawCode);
                }
                StateHasChanged();
            };
            _diagram.MouseClick += (model, args) =>
            {
                Console.WriteLine($"MouseClick, Type={model?.GetType().Name}, ModelId={model?.Id}");
                if (model is SyntaxNode node)
                {
                    SendCode.InvokeAsync(node.RawCode);
                }
            };
        }
        private List<SyntaxNode> CreateClassMemberNodes(ClassInfo cls, NodeModel node = null)
        {
            
            var classNode = new SyntaxNode(new Point(200 * _rowColumns[cls.RootLevel], 220 * cls.RootLevel))
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
            var classMembers = new List<SyntaxNode>();
            foreach (var methodNode in cls.Methods.Select(CreateMethodNode))
            {
                _nodeLinks.Add(new LinkModel(classNode.GetPort(PortAlignment.Bottom), methodNode.GetPort(PortAlignment.Top)));
                classMembers.Add(methodNode);
            }
            cls.NestedClasses.ForEach(x => CreateClassMemberNodes(x, classNode));

            foreach (var propNode in cls.Properties.Select(CreatePropNode))
            {
                _nodeLinks.Add(new LinkModel(classNode.GetPort(PortAlignment.Bottom), propNode.GetPort(PortAlignment.Top)));
                classMembers.Add(propNode);
            }
            foreach (var fieldNode in cls.Fields.Select(CreateFieldNode))
            {
                _nodeLinks.Add(new LinkModel(classNode.GetPort(PortAlignment.Bottom), fieldNode.GetPort(PortAlignment.Top)));
                classMembers.Add(fieldNode);
            }

            return classMembers;
        }

        private SyntaxNode CreateFieldNode(PropertyInfo field)
        {
            var fieldNode = new SyntaxNode(new Point(220 * _rowColumns[field.RootLevel + 1], 220 * field.RootLevel + 1))
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
            var propNode = new SyntaxNode(new Point(220 * _rowColumns[prop.RootLevel + 1], 220 * prop.RootLevel + 1))
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
            var methodNode = new SyntaxNode(new Point(220 * _rowColumns[method.RootLevel], 220 * method.RootLevel))
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

        private SyntaxNode CreateGlobalNode(GlobalDeclarationInfo global)
        {
            var node = new SyntaxNode(new Point(280 * _rowColumns[global.RootLevel], 220 * global.RootLevel))
            {
                Name = global.Name,
                RawCode = global.RawCode,
                Type = global.Type,
                NodeKind = NodeKind.Global
            };
            _rowColumns[global.RootLevel]++;
            node.AddNodePorts();
            _globalNodes.Add(node);
            return node;
        }
    }
}
