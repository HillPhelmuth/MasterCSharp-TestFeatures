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
        private FullSyntaxTree _fullSyntaxTree;

        public FullSyntaxTree FullSyntaxTree
        {
            get => _fullSyntaxTree; 
            set
            {
                _fullSyntaxTree = value;
                this.Title = _fullSyntaxTree.Kind;
            }
        }
        public bool IsExpanded { get; set; }
        public List<string> ChildrenIds { get; set; } = new();
    }
}
