using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;

namespace DTEngine.Entities
{
    public class StiffnessMatrix
    {
        public decimal[,] LocalStiffnessMatrix { get; init; } = new decimal[5, 5];
        public decimal[,] GlobalStiffnessMatrix { get => GetGlobalMatrix(); }

        private ComputationalDomainParams domainParams;
        private NodeMap nodeMap;

        private decimal[,]? globalStiffnessMatrix;

        public StiffnessMatrix(decimal conductingFactor, ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            var lambda = conductingFactor / 6m;

            LocalStiffnessMatrix[1, 1] = lambda * 4;
            LocalStiffnessMatrix[1, 2] = lambda * -1;
            LocalStiffnessMatrix[1, 3] = lambda * -2;
            LocalStiffnessMatrix[1, 4] = lambda * -1;

            LocalStiffnessMatrix[2, 1] = lambda * -1;
            LocalStiffnessMatrix[2, 2] = lambda * 4;
            LocalStiffnessMatrix[2, 3] = lambda * -1;
            LocalStiffnessMatrix[2, 4] = lambda * -2;

            LocalStiffnessMatrix[3, 1] = lambda * -2;
            LocalStiffnessMatrix[3, 2] = lambda * -1;
            LocalStiffnessMatrix[3, 3] = lambda * 4;
            LocalStiffnessMatrix[3, 4] = lambda * -1;

            LocalStiffnessMatrix[4, 1] = lambda * -1;
            LocalStiffnessMatrix[4, 2] = lambda * -2;
            LocalStiffnessMatrix[4, 3] = lambda * -1;
            LocalStiffnessMatrix[4, 4] = lambda * 4;

            LocalStiffnessMatrix.Dump("LOCAL STIFFNESS MATRIX:",1,5);

            globalStiffnessMatrix = null;
            this.domainParams = domainParams;
            this.nodeMap = nodeMap;
        }

        private decimal[,] GetGlobalMatrix()
        {
            globalStiffnessMatrix ??= GenerateGlobalMatrix();
         
            return globalStiffnessMatrix;
        }

        public decimal[,] GenerateGlobalMatrix()
        {
            var KGlobal = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
            for (int elementId = 1; elementId <= domainParams.NumberOfElements; elementId++)
            {
                for (int firstNodeLocalId = 1; firstNodeLocalId <= 4; firstNodeLocalId++)
                {
                    var firstNodeGlobalId = nodeMap.GetNodeByLocalAddress(elementId, firstNodeLocalId).GlobalId;

                    for (int secondNodeLocalId = 1; secondNodeLocalId <= 4; secondNodeLocalId++)
                    {
                        var secondNodeGlobalId = nodeMap.GetNodeByLocalAddress(elementId, secondNodeLocalId).GlobalId;

                        if (secondNodeGlobalId > 0)
                            KGlobal[firstNodeGlobalId, secondNodeGlobalId] = KGlobal[firstNodeGlobalId, secondNodeGlobalId] + LocalStiffnessMatrix[firstNodeLocalId, secondNodeLocalId];
                    }
                }
            }

            return KGlobal;
        }
    }
}
