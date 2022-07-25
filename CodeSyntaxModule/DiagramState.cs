﻿using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core;
using MasterCsharpHosted.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSyntaxModule
{
    public class DiagramState
    {
        private Diagram _diagram;
        public DiagramState(Diagram diagram)
        {
            _diagram = diagram;
        }
        public event Action<string> OnSendRawCode;
        public event Action UpdateLayout;
        public void SendCode(string code) => SendRawCode(code);
        private void SendRawCode(string code)
        {
            OnSendRawCode?.Invoke(code);
        }
        public void AddChildNodes(SimpleNodeModel node)
        {
            foreach (var subItem in node.SimpleSyntaxTree.Members)
            {
                var subNode = new SimpleNodeModel() { SimpleSyntaxTree = subItem };
                subNode.AddNodePorts();
                var link = new LinkModel(node.GetPort(PortAlignment.Bottom), subNode.GetPort(PortAlignment.Top));
                _diagram.Nodes.Add(subNode);
                _diagram.Links.Add(link);
                node.ChildrenIds.Add(subNode.Id);
                //if (subItem.Members.Any())
                //{
                //    AddChildNodes(subItem, subNode);
                //}
            }
            UpdateLayout.Invoke();
        }
        public void RemoveChildNodes(SimpleNodeModel nodeModel)
        {
            if (!nodeModel.SimpleSyntaxTree.Members.Any()) return;
            RemoveChildren(nodeModel);

            UpdateLayout.Invoke();
        }

        private void RemoveChildren(SimpleNodeModel nodeModel)
        {
            var childNodes = _diagram.Nodes.Where(x => nodeModel.ChildrenIds.Contains(x.Id)).ToList();
            foreach (var node in childNodes)
            {
                _diagram.Nodes.Remove(node);
                if (node is SimpleNodeModel model && model.ChildrenIds.Any())
                {
                    RemoveChildren(model);
                    model.ChildrenIds.Clear();
                }
            }
        }
    }
}
