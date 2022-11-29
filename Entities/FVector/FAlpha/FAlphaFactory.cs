using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;

namespace DTEngine.Entities.FVector.FAlpha
{
    public class FAlphaFactory : IFAlphaFactory
    {
        private readonly ComputationalDomainParams domainParams;
        private readonly NodeMap nodeMap;

        public FAlphaFactory(ComputationalDomainParams domainParams, NodeMap nodeMap)
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
                if (clipsDown.Contains(element))       //boundary conditions bottom edges    
                {
                    falfaValue = heatExchangingFactor1 * temperatureBottom * domainParams.WidthStep / 2;
                    falfa_e[1, 1] = falfaValue * 1;
                    falfa_e[2, 1] = falfaValue * 1;
                    falfa_e[3, 1] = 0;
                    falfa_e[4, 1] = 0;
                }
                else if (clipsUp.Contains(element))       //boundary conditions top edges       
                {
                    falfaValue = heatExchangingFactor1 * temperatureTop * domainParams.WidthStep / 2;
                    falfa_e[1, 1] = 0;
                    falfa_e[2, 1] = 0;
                    falfa_e[3, 1] = falfaValue * 1;
                    falfa_e[4, 1] = falfaValue * 1;
                }
                else if (element > 22)       //boundary conditions top middle     
                {
                    falfaValue = heatExchangingFactor2 * temperatureTop * domainParams.WidthStep / 2;
                    falfa_e[1, 1] = 0;
                    falfa_e[2, 1] = 0;
                    falfa_e[3, 1] = falfaValue * 1;
                    falfa_e[4, 1] = falfaValue * 1;
                }
                else       //boundary conditions bottom middle
                {
                    falfaValue = heatExchangingFactor2 * temperatureBottom * domainParams.WidthStep / 2;
                    falfa_e[1, 1] = falfaValue * 1;
                    falfa_e[2, 1] = falfaValue * 1;
                    falfa_e[3, 1] = 0;
                    falfa_e[4, 1] = 0;
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
