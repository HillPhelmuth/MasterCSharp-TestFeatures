using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Diagrams.Core;
using Microsoft.AspNetCore.Components;

namespace CodeSyntaxModule
{
    public partial class CSharpNode
    {
        [Parameter]
        public SyntaxNode Node { get; set; }
        [CascadingParameter(Name = "DiagramState")]
        public CSharpDiagramState DiagramState { get; set; }
        [CascadingParameter]
        public Diagram Diagram { get; set; }

    }
}
