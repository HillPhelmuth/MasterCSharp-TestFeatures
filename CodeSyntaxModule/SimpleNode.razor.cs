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

namespace CodeSyntaxModule;

public partial class SimpleNode
{
    [Inject]
    public AppState AppState { get; set; }
    [Parameter]
    public SimpleNodeModel Node { get; set; }
    [CascadingParameter]
    public Diagram Diagram { get; set; }
}