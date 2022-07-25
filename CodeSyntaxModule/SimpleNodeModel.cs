using Blazor.Diagrams.Core.Models;
using MasterCsharpHosted.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSyntaxModule
{
    public class SimpleNodeModel : NodeModel
    {
        private SimpleSyntaxTree simpleSyntaxTree;

        public SimpleSyntaxTree SimpleSyntaxTree
        {
            get => simpleSyntaxTree; 
            set
            {
                simpleSyntaxTree = value;
                this.Title = simpleSyntaxTree.Kind;
            }
        }
        public bool IsExpanded { get; set; }
        public List<string> ChildrenIds { get; set; } = new();
    }
}
