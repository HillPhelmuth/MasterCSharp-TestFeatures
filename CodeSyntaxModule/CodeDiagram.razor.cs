using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
//< !--required to resolve DiagramCanvas component -->

using GraphShape.Algorithms.Layout;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using QG = QuikGraph;

namespace CodeSyntaxModule
{
    public partial class CodeDiagram : ComponentBase, IDisposable
    {
        [Inject]
        public AppState AppState { get; set; }
        [Parameter]
        public SyntaxTreeInfo SyntaxTreeInfo { get; set; }
        [Parameter]
        public EventCallback<string> SendCode { get; set; }
        private List<string> Graphlist => new StandardLayoutAlgorithmFactory<string, QG.IEdge<string>, QG.IBidirectionalGraph<string, QG.IEdge<string>>>().AlgorithmTypes.ToList();
        private readonly string _selectedGraph = "Tree";
        public DiagramState DiagramState { get; set; }

        private readonly List<SyntaxNode> _namespaceNodes = new();
        private readonly List<SyntaxNode> _classNodes = new();
        private readonly List<SyntaxNode> _methodNodes = new();
        private readonly List<SyntaxNode> _propNodes = new();
        private readonly List<SyntaxNode> _fieldNodes = new();
        private readonly List<SyntaxNode> _globalNodes = new();
        private readonly List<SyntaxNode> _enumNodes = new();
        private readonly Dictionary<SyntaxNode, List<SyntaxNode>> _methodGroups = new();
        private readonly Dictionary<SyntaxNode, List<SyntaxNode>> _classGroups = new();
        private readonly Dictionary<SyntaxNode, List<SyntaxNode>> _enumGroups = new();
        private readonly Dictionary<SyntaxNode, List<SyntaxNode>> _namespaceGroups = new();
        private readonly List<LinkModel> _nodeLinks = new();

        private readonly List<SyntaxNode> _allNodes = new();

        private static readonly DiagramOptions _options = new()
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
                DefaultSelectedColor = "red",

            },
            EnableVirtualization = false,
            GridSize = 40,

        };
        private readonly List<SyntaxGroup> _groups = new();
        private readonly Diagram _diagram = new(_options);
        private readonly Dictionary<int, int> _rowColumns = new();
        private readonly bool _isExpandMethods;
        protected override Task OnInitializedAsync()
        {
            DiagramState = new DiagramState(_diagram);
            AppState.PropertyChanged += AppStateChanged;
            for (int i = 0; i < 100; i++) _rowColumns[i] = 0;


            RegisterEvents();
            InitDiagram();

            return base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                TryLayout();
                await Task.Delay(1);
                ZoomToFit();
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private void InitDiagram()
        {
            _diagram.RegisterModelComponent<SyntaxNode, CSharpNode>();
            _diagram.RegisterModelComponent<SyntaxGroup, SyntaxGroupWidget>();
            int nsIndex = 1;
            foreach (var nameSpace in SyntaxTreeInfo.NameSpaces)
            {
                var node = new SyntaxNode(nameSpace.Id, "", new Point((150 * _rowColumns[nameSpace.RootLevel]) + 175, 0))
                {
                    Name = nameSpace.Name,
                    RawCode = nameSpace.RawCode,
                    NodeKind = NodeKind.Namespace,
                    Row = nameSpace.RootLevel,
                    Column = _rowColumns[nameSpace.RootLevel]
                };
                _rowColumns[nameSpace.RootLevel]++;
                node.AddPort();
                _namespaceNodes.Add(node);
                _namespaceGroups[node] = new();
                foreach (var cls in nameSpace.Classes)
                {
                    _namespaceGroups[node].Add(AddClassAndMemeberNodes(cls, node));
                }
                foreach (var enm in nameSpace.Enums)
                {
                    _namespaceGroups[node].Add(AddEnumAndMemberNodes(enm));
                }
                nsIndex++;
            }

            foreach (var cls in SyntaxTreeInfo.Classes)
            {
                AddClassAndMemeberNodes(cls);
            }

            if (SyntaxTreeInfo.GlobalDeclarations.Count > 0)
            {
                SyntaxNode previousNode = null;
                var globalNode = new SyntaxNode("", "", new Point(200 * _rowColumns[1], 0))
                {
                    Name = "Syntax Tree",
                    RawCode = SyntaxTreeInfo.SourceCode,
                    NodeKind = NodeKind.None,
                    Row = 1,
                    Column = _rowColumns[1]
                };
                globalNode.AddNodePorts();

                foreach (var global in SyntaxTreeInfo.GlobalDeclarations.Select(g => CreateGlobalNode(g)))
                {
                    _globalNodes.Add(global);
                    if (previousNode != null)
                    {
                        _nodeLinks.Add(new LinkModel(previousNode.GetPort(PortAlignment.Right),
                            global.GetPort(PortAlignment.Left)));
                    }

                    _nodeLinks.Add(new LinkModel(globalNode.GetPort(PortAlignment.Bottom), global.GetPort(PortAlignment.Top)));
                    previousNode = global;
                }

                _diagram.Nodes.Add(globalNode);
            }

            if (SyntaxTreeInfo.Methods.Count > 0)
            {
                _methodNodes.AddRange(SyntaxTreeInfo.Methods.Select(x => CreateMethodNode(x)));
                _diagram.Nodes.Add(_methodNodes);
            }

            var allNodes = _namespaceNodes.Union(_classNodes).Union(_methodNodes).Union(_propNodes).Union(_fieldNodes)
                .Union(_globalNodes).Union(_enumNodes).ToList();
            
            AddGroupNodes(_namespaceGroups, NodeKind.Namespace);
            Console.WriteLine($"Class Groups: {_classGroups.Count}");
            AddGroupNodes(_classGroups, NodeKind.Class, 40);
            AddGroupNodes(_enumGroups, NodeKind.Enum);
            AddGroupNodes(_methodGroups, NodeKind.Method, 50);

            allNodes.ForEach(node =>
            {
                if (node.Row > 0) node.Row--;
                node.SetPosition(node.Column * 240, node.Row * 220);
            });

           
            _diagram.Nodes.Add(allNodes);

            _nodeLinks.ForEach(lnk =>
            {
                lnk.SourceMarker = LinkMarker.Arrow;
                lnk.TargetMarker = LinkMarker.Arrow;
            });
            _diagram.Links.Add(_nodeLinks);
            _groups.ForEach(g => { _diagram.AddGroup(g); });
            foreach (var lnk in _diagram.Links.OfType<LinkModel>())
            {
                Console.WriteLine($"Source: {lnk.SourceNode?.GetType().ToString() ?? "none"}\r\nTarget: {lnk.TargetNode?.GetType().ToString() ?? "none"}");
            }
        }

        private void AddGroupNodes(Dictionary<SyntaxNode, List<SyntaxNode>> nameSpGroup, NodeKind kind, int padding = 25)
        {
            foreach ((var parent, var nodeGroup) in nameSpGroup)
            {
                SyntaxGroup groupItem = new(nodeGroup, parent.Name, NodeKind.Namespace, 25);
                groupItem.AddNodePorts();
                _groups.Add(groupItem);
                _nodeLinks.AddRange(nodeGroup.Select(x => new LinkModel(parent, x)));
            }
        }

        private class LayoutOptions
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        private static readonly List<LayoutOptions> layoutOptions = GetLayoutOptions();
        private static List<LayoutOptions> GetLayoutOptions()
        {
            var algoFact = new StandardLayoutAlgorithmFactory<SyntaxNode, QG.Edge<SyntaxNode>, QG.BidirectionalGraph<SyntaxNode, QG.Edge<SyntaxNode>>>();
            return algoFact.AlgorithmTypes.Select(x => new LayoutOptions { Name = x, Value = x }).ToList();
        }
        private void TryLayout(LayoutOptions layout)
        {
            TryLayout(layout.Value);
        }
        private void TryLayout(string layout = "Sugiyama")
        {
            Console.WriteLine($"Layout updated to {layout}");
            var graph = new QG.BidirectionalGraph<SyntaxNode, QG.Edge<SyntaxNode>>();
            var nodes = _diagram.Nodes.OfType<SyntaxNode>().ToList();
            var groups = _diagram.Groups.OfType<SyntaxGroup>().ToList();
            var edges = _diagram.Links.OfType<LinkModel>().Select(lm =>
            {
                var source = nodes.FirstOrDefault(dn => dn.Id == lm.SourceNode.Id);
                var target = nodes.FirstOrDefault(dn => dn.Id == lm?.TargetNode?.Id);
                return new QG.Edge<SyntaxNode>(source, target);
            }).ToList();
            
            graph.AddVertexRange(nodes);
            graph.AddEdgeRange(edges);

            var positions = nodes.ToDictionary(nm => nm, dn => new GraphShape.Point(dn.Position.X, dn.Position.Y));
            var sizes = nodes.ToDictionary(nm => nm, dn => new GraphShape.Size(dn.Size?.Width + 25 ?? 100, dn.Size?.Height + 25 ?? 100));
            var layoutCtx = new LayoutContext<SyntaxNode, QG.Edge<SyntaxNode>, QG.BidirectionalGraph<SyntaxNode, QG.Edge<SyntaxNode>>>(graph, positions, sizes, LayoutMode.Simple);
            var algoFact = new StandardLayoutAlgorithmFactory<SyntaxNode, QG.Edge<SyntaxNode>, QG.BidirectionalGraph<SyntaxNode, QG.Edge<SyntaxNode>>>();
            var algo = algoFact.CreateAlgorithm(layout, layoutCtx, null);
            try
            {
                algo.Compute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION\r\n{ex.Message}");
            }

            try
            {
                _diagram.SuspendRefresh = true;
                foreach (var vertPos in algo.VerticesPositions)
                {
                    // NOTE;  have to use SetPosition which takes care of updating everything
                    vertPos.Key.SetPosition(vertPos.Value.X, vertPos.Value.Y);
                }
            }
            finally
            {
                _diagram.SuspendRefresh = false;
            }
        }
        private bool isShow;
        private void ToggleCodeWindow()
        {
            isShow = !isShow;
            AppState.ShowCode(isShow);
        }
        private void ZoomToFit()
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
                            Console.WriteLine($"Node Row={node.Row}, Column={node.Column}");
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
        private SyntaxNode AddClassAndMemeberNodes(ClassInfo cls, SyntaxNode parent = null)
        {
            var classNode = CreateClassNode(cls);
            _classNodes.Add(classNode);
            _classGroups[classNode] = CreateClassMemberNodes(cls, classNode);
            return classNode;

        }
        private SyntaxNode AddEnumAndMemberNodes(EnumInfo enm)
        {
            var enumNode = CreateEnumNode(enm);
            _enumNodes.Add(enumNode);
            _enumGroups[enumNode] = CreateEnumMemberNodes(enm);
            return enumNode;
        }
        private List<SyntaxNode> CreateClassMemberNodes(ClassInfo cls, SyntaxNode classNode = null)
        {
            var classMembers = new List<SyntaxNode>();
            foreach (var method in cls.Methods)
            {
                var methodNode = CreateMethodNode(method);
                _methodNodes.Add(methodNode);
                var methodGroup = CreateMethodMembersNodes(method);
                _methodGroups[methodNode] = methodGroup;

                classMembers.Add(methodNode);
            }
            foreach (var ctor in cls.Constructors)
            {
                var ctorNode = CreateMethodNode(ctor, isCtor: true);
                _methodNodes.Add(ctorNode);
                var ctorGroup = CreateMethodMembersNodes(ctor);
                _methodGroups[ctorNode] = ctorGroup;
                classMembers.Add(ctorNode);
            }
            foreach (var nested in cls.NestedClasses)
            {
                var nestedClass = CreateClassNode(nested);
                _classNodes.Add(nestedClass);
                _classGroups[nestedClass] = CreateClassMemberNodes(nested, nestedClass);
                classMembers.Add(nestedClass);
            }
            foreach (var enm in cls.Enums)
            {
                var enumNode = CreateEnumNode(enm);
                _enumNodes.Add(enumNode);
                _enumGroups[enumNode] = CreateEnumMemberNodes(enm);
                classMembers.Add(enumNode);
            }
            //cls.NestedClasses.ForEach(x => CreateClassMemberNodes(x, classNode));

            classMembers.AddRange(cls.Properties.Select(CreatePropNode));
            classMembers.AddRange(cls.Fields.Select(CreateFieldNode));

            return classMembers;
        }

        private List<SyntaxNode> CreateMethodMembersNodes(MethodInfo method)
        {
            var methods = new List<SyntaxNode>();
            foreach (var stmt in method.BodySyntax)
            {
                var node = CreateGlobalNode(stmt);

                methods.Add(node);
            }
            return methods;
        }

        private SyntaxNode CreateClassNode(ClassInfo cls)
        {
            var classNode = new SyntaxNode(cls.Id, cls.GroupId, new Point(220 * _rowColumns[cls.RootLevel], 220 * cls.RootLevel))
            {
                Name = cls.Name,
                RawCode = cls.RawCode,
                NodeKind = NodeKind.Class,
                Row = cls.RootLevel,
                Column = cls.Column
            };

            _rowColumns[cls.RootLevel]++;
            classNode.AddNodePorts();

            _classNodes.Add(classNode);
            Console.WriteLine("Class Node added");
            
            return classNode;
        }
        private SyntaxNode CreateEnumNode(EnumInfo enm)
        {
            var enumNode = new SyntaxNode(enm.Id, enm.GroupId, new Point(220 * _rowColumns[enm.RootLevel], 220 * enm.RootLevel))
            {
                Name = enm.Name,
                RawCode = enm.RawCode,
                NodeKind = NodeKind.Enum,
                Row = enm.RootLevel,
                Column = enm.Column
            };

            _rowColumns[enm.RootLevel]++;
            enumNode.AddNodePorts();

            _enumNodes.Add(enumNode);
            Console.WriteLine("Class Node added");

            return enumNode;
        }
        private List<SyntaxNode> CreateEnumMemberNodes(EnumInfo enm)
        {
            var result = new List<SyntaxNode>();
            List<SyntaxNode> syntaxNodes = enm.Fields.Select(CreateFieldNode).ToList();
            
            return syntaxNodes;
            
        }
        private SyntaxNode CreateFieldNode(PropertyInfo field)
        {
            return CreatePropOrFieldNode(field, NodeKind.Field);
        }

        private SyntaxNode CreatePropNode(PropertyInfo prop)
        {
            return CreatePropOrFieldNode(prop, NodeKind.Property);
        }

        private SyntaxNode CreatePropOrFieldNode(PropertyInfo prop, NodeKind kind)
        {
            var propNode = new SyntaxNode(prop.Id, prop.GroupId, new Point(240 * _rowColumns[prop.RootLevel + 1], 220 * prop.RootLevel + 1))
            {
                Name = prop.Name,
                RawCode = prop.RawCode,
                Type = prop.Type,
                NodeKind = kind,
                Row = prop.RootLevel,
                Column = prop.Column
            };
            _rowColumns[prop.RootLevel + 1]++;
            propNode.AddNodePorts();
            _propNodes.Add(propNode);
            Console.WriteLine("Property Node added");
            return propNode;
        }

        private SyntaxNode CreateMethodNode(MethodInfo method, bool isCtor = false)
        {
            var methodNode = new SyntaxNode(method.Id, method.GroupId, new Point(240 * _rowColumns[method.RootLevel], 220 * method.RootLevel))
            {
                Name = method.Name,
                RawCode = method.RawCode,
                Type = method.ReturnType,
                Parameters = method.Parameters,
                NodeKind = isCtor ? NodeKind.Constructor : NodeKind.Method,
                Row = method.RootLevel,
                Column = method.Column
            };
            _rowColumns[method.RootLevel]++;
            methodNode.AddNodePorts();
            _methodNodes.Add(methodNode);
            Console.WriteLine("Method Node added");
            return methodNode;
        }

        private SyntaxNode CreateGlobalNode(GlobalDeclarationInfo global, bool isMethodBody = false)
        {
            var node = new SyntaxNode(global.Id, global.GroupId, new Point(240 * _rowColumns[global.RootLevel], 220 * global.RootLevel))
            {
                Name = global.Name,
                RawCode = global.RawCode,
                Type = global.Type,
                NodeKind = NodeKind.Global,
                Row = global.RootLevel,
                Column = global.Column
            };
            if (isMethodBody) return node;
            _rowColumns[global.RootLevel]++;
            node.AddNodePorts();
            _globalNodes.Add(node);
            Console.WriteLine("Global Node added");
            return node;
        }

        private void AppStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AppState.SyntaxTreeInfo)) return;
            Console.WriteLine("CodeDiagram.razor State Changed");
            StateHasChanged();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            AppState.PropertyChanged -= AppStateChanged;
        }
    }
}
