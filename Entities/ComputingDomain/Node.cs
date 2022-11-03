using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.ComputingDomain
{
    public class Node
    {
        public int GlobalId { get; set; }
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }

        public Node(int globalId, decimal posX, decimal posY)
        {
            GlobalId = globalId;
            PosX = posX;
            PosY = posY;
        }
    }
}
