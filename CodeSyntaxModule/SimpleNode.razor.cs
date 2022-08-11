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
using Blazor.Diagrams.Core.Models.Base;

namespace CodeSyntaxModule;

public partial class SimpleNode
{
    [Inject]
    public AppState AppState { get; set; }
    [Parameter]
    public SimpleNodeModel Node { get; set; }
    [CascadingParameter]
    public Diagram Diagram { get; set; }
    [CascadingParameter]
    public DiagramState DiagramState { get; set; }

    private string expandCollapseCss => Node.IsExpanded ? "collapsed-icon" : "expand-icon";
    private void HandleExpandMembers()
    {
        if (Node.IsExpanded)
        {
            Node.IsExpanded = false;
            DiagramState.RemoveChildNodes(Node);
        }
        else
        {
            Node.IsExpanded = true;
            DiagramState.AddChildNodes(Node);
        }
    }

    private void HandleExpandAll()
    {
        if (Node.IsExpanded) return;
        Node.IsExpanded = true;
        DiagramState.AddChildNodes(Node, true);
    }
}
public static class NodeExtensions
{
    public static string HasMembersCss(this SimpleNodeModel model)
    {
        return model.FullSyntaxTree.Members.Any() ? "expand" : "";
    }
}