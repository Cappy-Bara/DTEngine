using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.FVector
{
    public class FQSmallFactory
    {
        private readonly ComputationalDomainParams domainParams;
        private readonly NodeMap nodeMap;

        public FQSmallFactory(ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            this.domainParams = domainParams;
            this.nodeMap = nodeMap;
        }

        public decimal[,] Generate(decimal heatStream)
        {
            var fq = new decimal[251, 2];

            if (heatStream != 0.0m)
            {
                decimal fq_value;
                var fq_e = new decimal[5, 2];

                for (int element = 1; element <= domainParams.NumberOfElements; element++)          //to one loop
                {
                    var xe = new decimal[5];
                    var ye = new decimal[5];

                    for (int j = 1; j <= 4; j++)            //to remove
                    {
                        xe[j] = nodeMap.GetNodeByLocalAddress(element, j).PosX;
                        ye[j] = nodeMap.GetNodeByLocalAddress(element, j).PosY;
                    }
                    if (element == 1 || element == 3)       //boundary conditions left     
                    {
                        fq_value = (heatStream * (ye[4] - ye[1])) / 2;
                        fq_e[1, 1] = fq_value * 1;
                        fq_e[2, 1] = 0;
                        fq_e[3, 1] = 0;
                        fq_e[4, 1] = fq_value * 1;
                    }
                    if (element == 2 || element == 4)       //other elements
                    {
                        fq_e[1, 1] = 0; fq_e[2, 1] = 0;
                        fq_e[3, 1] = 0; fq_e[4, 1] = 0;
                    }
                    for (int i = 1; i <= 4; i++)          //falfa building
                    {
                        var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
                        if (ii > 0)
                            fq[ii, 1] = fq[ii, 1] + fq_e[i, 1];
                    }
                }
            }

            return fq.DumpVector("Fq VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);
        }
    }
}
