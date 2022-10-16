using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Contracts
{
    public class NodePosition
    {
        public int ElementNumber { get; set; }
        public int LocalAddress { get; set; }
        public int GlobalAddress { get; set; }
    }
}
