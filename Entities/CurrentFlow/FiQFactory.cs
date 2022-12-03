using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;


namespace DTEngine.Entities.CurrentFlow
{
    public class FiQFactory
    {
        private readonly ComputationalDomainParams domainParams;
        private readonly NodeMap nodeMap;

        public FiQFactory(ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            this.domainParams = domainParams;
            this.nodeMap = nodeMap;
        }

        public decimal[,] Generate(decimal[,] heatSourcePower)
        {
            var fQ = new decimal[251, 2];
            var fQElement = new decimal[5, 2];
            decimal fQValue1;
            decimal fQValue2;
            decimal fQValue3;
            decimal fQValue4;

            for (int element = 1; element <= domainParams.NumberOfElements; element++)
            {
                fQValue1 = heatSourcePower[nodeMap.GetNodeByLocalAddress(element, 1).GlobalId, 1] * domainParams.WidthStep * domainParams.HeightStep / 4.0m;
                fQValue2 = heatSourcePower[nodeMap.GetNodeByLocalAddress(element, 2).GlobalId, 1] * domainParams.WidthStep * domainParams.HeightStep / 4.0m;
                fQValue3 = heatSourcePower[nodeMap.GetNodeByLocalAddress(element, 3).GlobalId, 1] * domainParams.WidthStep * domainParams.HeightStep / 4.0m;
                fQValue4 = heatSourcePower[nodeMap.GetNodeByLocalAddress(element, 4).GlobalId, 1] * domainParams.WidthStep * domainParams.HeightStep / 4.0m;

                fQElement[1, 1] = fQValue1 * 1m; fQElement[2, 1] = fQValue2 * 1m;
                fQElement[3, 1] = fQValue3 * 1m; fQElement[4, 1] = fQValue4 * 1m;

                for (int i = 1; i <= 4; i++)
                {
                    var ii = nodeMap.GetNodeByLocalAddress(element, i).GlobalId;
                    if (ii > 0)
                        fQ[ii, 1] = fQ[ii, 1] + fQElement[i, 1];
                }
            }

            return fQ.DumpVector("FiQ VECTOR:", 1, 1, domainParams.NumberOfNodes + 1);
        }
    }
}
