using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace CodeSyntaxModule
{
    public class SyntaxNode : NodeModel
    {
        public SyntaxNode(Point position = null) : base(position) { }
        public string Name { get; set; }
        public string RawCode { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public NodeKind NodeKind { get; set; }
        
    }

    public enum NodeKind
    {
        None,
        Namespace,
        Class,
        Method,
        Property,
        Field,
        Global
    }

    public static class Extensions
    {
        public static void AddNodePorts(this NodeModel node, bool top = true, bool bottom = true, bool left = true,
            bool right = true)
        {
            if (top) node.AddPort(PortAlignment.Top);
            if (bottom) node.AddPort();
            if (left) node.AddPort(PortAlignment.Left);
            if (right) node.AddPort(PortAlignment.Right);
        }
        
    }
   
}
