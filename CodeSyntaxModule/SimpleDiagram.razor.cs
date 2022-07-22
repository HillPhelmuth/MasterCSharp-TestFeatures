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
            GridSize = 50,

        };
        
        private readonly Diagram _diagram = new(_options);
        private readonly List<LinkModel> _nodeLinks = new();
        protected override Task OnInitializedAsync()
        {
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
        private void RegisterEvents()
        {
            _diagram.SelectionChanged += (selected) =>
            {
                if (selected is SimpleNodeModel model)
                {
                    SendCode.InvokeAsync(model.SimpleSyntaxTree?.RawCode ?? "No Code");
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
                AddChildNodes(treeItem, node);
            }
        }

        private void AddChildNodes(SimpleSyntaxTree treeItem, SimpleNodeModel node)
        {
            foreach (var subItem in treeItem.Members)
            {
                var subNode = new SimpleNodeModel() { SimpleSyntaxTree = subItem };
                subNode.AddNodePorts();
                var link = new LinkModel(node.GetPort(PortAlignment.Bottom), subNode.GetPort(PortAlignment.Top));
                _diagram.Nodes.Add(subNode);
                _diagram.Links.Add(link);
                if (subItem.Members.Any())
                {
                    AddChildNodes(subItem, subNode);
                }
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

            var positions = nodes.ToDictionary(nm => nm, dn => new GraphShape.Point(dn.Position.X, dn.Position.Y));
            var sizes = nodes.ToDictionary(nm => nm, dn => new GraphShape.Size(dn.Size?.Width + 25 ?? 100, dn.Size?.Height + 25 ?? 100));
            var layoutCtx = new LayoutContext<SimpleNodeModel, QG.Edge<SimpleNodeModel>, QG.BidirectionalGraph<SimpleNodeModel, QG.Edge<SimpleNodeModel>>>(graph, positions, sizes, LayoutMode.Simple);
            var algoFact = new StandardLayoutAlgorithmFactory<SimpleNodeModel, QG.Edge<SimpleNodeModel>, QG.BidirectionalGraph<SimpleNodeModel, QG.Edge<SimpleNodeModel>>>();
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

        #endregion
    }
}