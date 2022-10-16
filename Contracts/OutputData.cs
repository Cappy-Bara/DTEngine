using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Contracts
{
    public class OutputData
    {
        public IEnumerable<NodePosition> Nodes { get; set; }

        public OutputData(int[,] addresses, int elementsQuantity)
        {
            var output = new List<NodePosition>();

            for (int i = 1; i <= elementsQuantity; i++)
            {
                output.Add(new NodePosition()
                {
                    GlobalAddress = addresses[i,1],
                    LocalAddress = 1,
                    ElementNumber = i,
                });

                output.Add(new NodePosition()
                {
                    GlobalAddress = addresses[i, 2],
                    LocalAddress = 2,
                    ElementNumber = i,
                });

                output.Add(new NodePosition()
                {
                    GlobalAddress = addresses[i, 3],
                    LocalAddress = 3,
                    ElementNumber = i,
                });

                output.Add(new NodePosition()
                {
                    GlobalAddress = addresses[i, 4],
                    LocalAddress = 4,
                    ElementNumber = i,
                });
            }

            Nodes = output;
        }
    }
}
