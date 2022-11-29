using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.FVector.FAlpha
{
    public class FAlphaFactoryForTest : IFAlphaFactory
    {
        private ComputationalDomainParams domainParams;
        private NodeMap nodeMap;

        public FAlphaFactoryForTest(ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            this.domainParams = domainParams;
            this.nodeMap = nodeMap;
        }

        public decimal[,] Generate(decimal temperatureTop, decimal temperatureBottom,
            int[] clipsUp, int[] clipsDown, decimal heatExchangingFactor1, decimal heatExchangingFactor2)
        {
            decimal falfaValue;
            var falfa_e = new decimal[5, 2];
            var falfa = new decimal[251, 2];


            for (int element = 1; element <= domainParams.NumberOfElements; element++)
            {

                if (element == 1 || element == 2)       //boundary conditions bottom       
                {
                    falfaValue = ((heatExchangingFactor1 * temperatureBottom) * (domainParams.WidthStep)) / 2;
                    falfa_e[1, 1] = falfaValue * 1;
                    falfa_e[2, 1] = falfaValue * 1;
                    falfa_e[3, 1] = 0;
                    falfa_e[4, 1] = 0;
                }
                if (element == 3 || element == 4)      //boundary conditions top        
                {
                    falfa_e[1, 1] = 0; falfa_e[2, 1] = 0;
                    falfa_e[3, 1] = 0; falfa_e[4, 1] = 0;
                }

                for (int i = 1; i <= 4; i++)          //falfa building
                {
                    var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
                    if (ii > 0)
                        falfa[ii, 1] = falfa[ii, 1] + falfa_e[i, 1];
                }
            }

            return falfa.DumpVector("FALPHA VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);
        }
    }
}
