using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.ComputingDomain
{
    public class Element
    {
        public int Id { get; set; }
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }
        public Node Node3 { get; set; }
        public Node Node4 { get; set; }

        public Element(int id, Node node1, Node node2, Node node3, Node node4)
        {
            Id = id;
            Node1 = node1;
            Node2 = node2;
            Node3 = node3;
            Node4 = node4;
        }

        public Node GetNodeByLocalId(int nodeNumber)
        {
            switch (nodeNumber)
            {
                case 1: return Node1;
                case 2: return Node2;
                case 3: return Node3;
                case 4: return Node4;
                default: throw new Exception("Invalid local node Id");
            }
        }
    }
}
