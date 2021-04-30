using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;

namespace CodeSyntaxModule
{
    public class SyntaxGroup : GroupModel
    {
        public SyntaxGroup(IEnumerable<NodeModel> children, string title, NodeKind kind, byte padding = 20)
            : base(children, padding)
        {
            Title = title;
            Kind = kind;
        }

        public string Title { get; }
        public NodeKind Kind { get; }
    }
}
