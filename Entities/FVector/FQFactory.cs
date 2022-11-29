using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.FVector
{
    public class FQFactory
    {
        private readonly ComputationalDomainParams domainParams;
        private readonly NodeMap nodeMap;

        public FQFactory(ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            this.domainParams = domainParams;
            this.nodeMap = nodeMap;
        }

        public decimal[,] Generate(decimal heatSourcePower)
        {
            var fQ = new decimal[251, 2];
            var fQElement = new decimal[5, 2];
            decimal fQValue;

            for (int element = 1; element <= domainParams.NumberOfElements; element++)
            {
                fQValue = heatSourcePower * domainParams.WidthStep * domainParams.HeightStep / 4.0m;

                fQElement[1, 1] = fQValue * 1m; fQElement[2, 1] = fQValue * 1m;
                fQElement[3, 1] = fQValue * 1m; fQElement[4, 1] = fQValue * 1m;

                for (int i = 1; i <= 4; i++)
                {
                    var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
                    if (ii > 0)
                        fQ[ii, 1] = fQ[ii, 1] + fQElement[i, 1];
                }
            }

            return fQ.DumpVector("FQ VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);
        }
    }
}
