using DTEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.ComputingDomain
{
    public class NodeMap
    {
        public Dictionary<int, Element> Elements = new();
        public Dictionary<int, Node> Nodes = new();
        private NodeFactory nodeFactory { get; set; }

        public NodeMap(ComputationalDomainParams domainParams)
        {
            nodeFactory = new NodeFactory(domainParams);

            for (int verticalPosition = 1, horizontalPosition = 1, elementNumber = 1;
                elementNumber <= domainParams.NumberOfElements;
                elementNumber++)
            {
                var global1 = verticalPosition;
                var global2 = 1 + verticalPosition;
                var global3 = 1 + verticalPosition + domainParams.HorizontalElementsQuantity;
                var global4 = verticalPosition + domainParams.HorizontalElementsQuantity;

                var node1 = GetNode(global1);
                var node2 = GetNode(global2);
                var node3 = GetNode(global3);
                var node4 = GetNode(global4);

                var element = new Element(elementNumber, node1, node2, node3, node4);
                
                Nodes.TryAdd(global1, node1);
                Nodes.TryAdd(global2, node2);
                Nodes.TryAdd(global3, node3);
                Nodes.TryAdd(global4, node4);
                
                Elements.Add(elementNumber, element);

                horizontalPosition++;
                verticalPosition++;

                if (horizontalPosition == domainParams.HorizontalElementsQuantity)
                {
                    horizontalPosition = 1;
                    verticalPosition++;
                }
            }
        }

        private Node GetNode(int globalId)
        {
            return GetNodeByGlobalAddress(globalId) ?? nodeFactory.CreateNode(globalId);
        }


        public Node GetNodeByLocalAddress(int elementId, int nodeId)
        {
            return Elements[elementId].GetNodeByLocalId(nodeId);
        }
        public Node? GetNodeByGlobalAddress(int nodeId)
        {
            Nodes.TryGetValue(nodeId, out var node);
            return node;
        }
        public Element GetElementById(int id)
        {
            return Elements[id];
        }
    }
}
