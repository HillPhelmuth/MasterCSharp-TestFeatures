using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Components;
using SharedComponents;
using MasterCsharpHosted.Shared;
using GraphShape.Algorithms.Layout;
using QG = QuikGraph;

namespace CodeSyntaxModule
{
    public partial class SimpleDiagram
    {
        [Inject]
        public AppState AppState { get; set; }
        [Parameter]
        public EventCallback<string> SendCode { get; set; }
        public DiagramState DiagramState { get; set; }
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
            GridSize = 20,

        };
        
        private readonly Diagram _diagram = new(_options);
        private readonly List<LinkModel> _nodeLinks = new();
        protected override Task OnInitializedAsync()
        {
            DiagramState = new DiagramState(_diagram);
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
        private List<string> expandedNodes = new();
        private void RegisterEvents()
        {
            DiagramState.UpdateLayout += async () =>
            {
                TryLayout();
                await InvokeAsync(StateHasChanged);
            };
            _diagram.SelectionChanged += async (selected) =>
            {
                if (selected is SimpleNodeModel model)
                {
                    //if (!model.Selected) return;
                    //if (model.IsExpanded)
                    //{
                    //    RemoveChildNodes(model);
                    //    model.IsExpanded = false;
                    //    TryLayout();
                    //    await InvokeAsync(StateHasChanged);
                    //}
                    //else if (model.SimpleSyntaxTree.Members.Any())
                    //{
                    //    //expandedNodes.Add(model.Id);
                    //    model.IsExpanded = true;
                    //    AddChildNodes(model.SimpleSyntaxTree, model);
                    //    TryLayout();
                    //    await InvokeAsync(StateHasChanged);
                    //}
                    
                    await SendCode.InvokeAsync(model.SimpleSyntaxTree?.RawCode ?? "No Code");
                    Console.WriteLine($"POSITION: {model.Position?.X}, {model.Position?.Y}");
                    

                }
            };
        }
        private void InitDiagram()
        {
            _diagram.RegisterModelComponent<SimpleNodeModel, SimpleNode>();
            foreach (var treeItem in AppState.SimpleSyntaxTrees)
            {
                var node = new SimpleNodeModel() { SimpleSyntaxTree = treeItem };
                node.AddNodePorts();
                _diagram.Nodes.Add(node);
                //AddChildNodes(treeItem, node);
            }
        }
        private void ExpandAll()
        {
            _diagram.Nodes.Clear();
            foreach (var treeItem in AppState.SimpleSyntaxTrees)
            {
                var node = new SimpleNodeModel() { SimpleSyntaxTree = treeItem, IsExpanded = true };
                node.AddNodePorts();
                _diagram.Nodes.Add(node);
                AddChildNodes(treeItem, node);
            }
            TryLayout();
        }
        private void CollapseAll()
        {
            _diagram.Nodes.Clear();
            foreach (var treeItem in AppState.SimpleSyntaxTrees)
            {
                var node = new SimpleNodeModel() { SimpleSyntaxTree = treeItem, IsExpanded = false };
                node.AddNodePorts();
                _diagram.Nodes.Add(node);
            }
        }
        private void AddChildNodes(SimpleSyntaxTree treeItem, SimpleNodeModel node)
        {
            foreach (var subItem in treeItem.Members)
            {
                var subNode = new SimpleNodeModel() { SimpleSyntaxTree = subItem, IsExpanded = true };
                subNode.AddNodePorts();
                var link = new LinkModel(node.GetPort(PortAlignment.Bottom), subNode.GetPort(PortAlignment.Top));
                _diagram.Nodes.Add(subNode);
                _diagram.Links.Add(link);
                node.ChildrenIds.Add(subNode.Id);
                if (subItem.Members.Any())
                {
                    AddChildNodes(subItem, subNode);
                }
            }
        }
        private void RemoveChildNodes(SimpleNodeModel nodeModel)
        {
            if (!nodeModel.SimpleSyntaxTree.Members.Any()) return;
            var childNodes = _diagram.Nodes.Where(x => nodeModel.ChildrenIds.Contains(x.Id)).ToList();
            foreach (var node in childNodes)
            {
                _diagram.Nodes.Remove(node);
            }
        }

        private void ZoomToFit()
        {
            _diagram.ZoomToFit(50);
        }
        private bool isShow;
        private void ToggleCodeWindow()
        {
            isShow = !isShow;
            AppState.ShowCode(isShow);
        }
        #region Graph Algo Implementation

        private void TryLayout(string layout = "Tree")
        {
            Console.WriteLine($"Layout updated to {layout}");
            var graph = new QG.BidirectionalGraph<SimpleNodeModel, QG.Edge<SimpleNodeModel>>();
            var nodes = _diagram.Nodes.OfType<SimpleNodeModel>().ToList();
            var groups = _diagram.Groups.OfType<SyntaxGroup>().ToList();
            var edges = _diagram.Links.OfType<LinkModel>().Select(lm =>
            {
                var source = nodes.FirstOrDefault(dn => dn.Id == lm.SourceNode.Id);
                var target = nodes.FirstOrDefault(dn => dn.Id == lm?.TargetNode?.Id);
                return new QG.Edge<SimpleNodeModel>(source, target);
            }).ToList();

            graph.AddVertexRange(nodes);
            graph.AddEdgeRange(edges);
#if DEBUG
            var nodeSizeDic = nodes?.Select(x => $"Title: {x?.Title}, Size: X-{x?.Position?.X ?? .999} Y-{x?.Position?.Y ?? .999}");
            Console.WriteLine($"Node Positions BEFORE: {string.Join("| ", nodeSizeDic.Take(30))}");
#endif
            var positions = nodes.ToDictionary(nm => nm, dn => new GraphShape.Point(dn.Position.X, dn.Position.Y));
            var sizes = nodes.ToDictionary(nm => nm, dn => new GraphShape.Size(dn.Size?.Width + 5 ?? 160, dn.Size?.Height + 15 ?? 60));
            var layoutCtx = new LayoutContext<SimpleNodeModel, QG.Edge<SimpleNodeModel>, QG.BidirectionalGraph<SimpleNodeModel, QG.Edge<SimpleNodeModel>>>(graph, positions, sizes, LayoutMode.Simple);
            var algoFact = new StandardLayoutAlgorithmFactory<SimpleNodeModel, QG.Edge<SimpleNodeModel>, QG.BidirectionalGraph<SimpleNodeModel, QG.Edge<SimpleNodeModel>>>();
            var algo = algoFact.CreateAlgorithm(layout, layoutCtx, null);

            try
            {
                algo.Compute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION\r\n{ex.Message}\r\n{ex.StackTrace}");
            }

            try
            {
                _diagram.SuspendRefresh = true;
//#if DEBUG
//                var nodeSizeDicAfter = nodes?.Select(x => $"Title: {x?.Title}, Size: X-{x?.Position?.X ?? .999} Y-{x?.Position?.Y ?? .999}");
//                Console.WriteLine($"Node Positions BEFORE: \r\n{string.Join("| ", nodeSizeDicAfter)}");
//                var graphVertPos = algo.VerticesPositions?.Select(x => $"{x.Key.Title} -- Position: X-{x.Value.X} Y-{x.Value.Y}");
//                Console.WriteLine($"EXPECTED POSITIONS: \r\n{string.Join("| ", graphVertPos)}");
//#endif
                foreach (var vertPos in algo.VerticesPositions)
                {
                    // NOTE;  have to use SetPosition which takes care of updating everything
                    vertPos.Key.SetPosition(vertPos.Value.X, vertPos.Value.Y);
                }
                //nodeSizeDicAfter = nodes?.Select(x => $"Title: {x?.Title}, Size: X-{x?.Position?.X ?? .999} Y-{x?.Position?.Y ?? .999}");
                //Console.WriteLine($"Node Positions AFTER: \r\n{string.Join("| ", nodeSizeDicAfter)}");
                
            }
            finally
            {
                _diagram.SuspendRefresh = false;
            }
        }

        #endregion
    }
}