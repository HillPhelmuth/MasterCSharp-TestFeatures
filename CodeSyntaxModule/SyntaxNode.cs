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
        public SyntaxNode(string nodeId, string groupId, Point position = null) : base(position) 
        {
            GroupId = groupId;
            NodeId = nodeId;
        }
        public string Name { get; set; }
        public string RawCode { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public NodeKind NodeKind { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsAdjusted { get; set; }
        public string GroupId { get; }
        public string NodeId { get; }
        
    }

    public enum NodeKind
    {
        None,
        Namespace,
        Class,
        Method,
        Property,
        Field,
        Global,
        NestedClass,
        Constructor,
        Enum
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

        public static List<SyntaxNode> AdjustRowColumns(this IEnumerable<SyntaxNode> nodes)
        {
            var newNodes = nodes.ToList();
            var rows = newNodes.Max(x => x.Row);
            var listRows = new List<int>();
            for (int i = 1; i <= rows; i++)
            {
                if (newNodes.Count(n => n.Row == i) <= 8) continue;

                int adjustedColumn = 1;
                int adjustedRow = 1;
                foreach (var node in newNodes.Where(n => n.Row == i))
                {
                    if (node.Column <= 8) continue;
                    node.Column = adjustedColumn;
                    node.Row += adjustedRow;
                    node.IsAdjusted = true;
                    listRows.Add(i + adjustedRow);
                    if (adjustedColumn == 8)
                    {
                        adjustedColumn = 1;
                        adjustedRow++;
                    }
                    else
                    {
                        adjustedColumn++;
                    }

                }

            }
            var rowsToAdjust = listRows.Distinct();
            newNodes.Where(x => rowsToAdjust.Contains(x.Row) && !x.IsAdjusted).ToList().ForEach(x => x.Row++);
            foreach (var node in newNodes)
            {
                node.Position = new Point(node.Column * 240, node.Row * 240);
            }
            return newNodes;
        }
        
    }
   
}
