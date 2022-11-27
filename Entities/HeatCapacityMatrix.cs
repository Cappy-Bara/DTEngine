using DTEngine.Entities.ComputingDomain;

namespace DTEngine.Entities
{
    public class HeatCapacityMatrix
    {
        public decimal[,] LocalHeatCapacityMatrix { get; init; } = new decimal[5, 5];
        public decimal[,] GlobalHeatCapacityMatrix { get => GetGlobalMatrix(); }

        private ComputationalDomainParams domainParams;
        private NodeMap nodeMap;

        private decimal[,]? globalStiffnessMatrix;

        public HeatCapacityMatrix(decimal heatCapacity, decimal density, decimal width, decimal height, ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            var baseHeatCapacityMatrix = new decimal[,] { { 0, 0, 0, 0, 0 }, { 0, 4, 2, 1, 2 }, { 0, 2, 4, 2, 1 }, { 0, 1, 2, 4, 2 }, { 0, 2, 1, 2, 4 } };
            var heatCapacityFactor = heatCapacity * density * width * height;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    LocalHeatCapacityMatrix[i, j] = baseHeatCapacityMatrix[i + 1, j + 1] * heatCapacityFactor;
                }
            }

            LocalHeatCapacityMatrix[1, 1] = heatCapacityFactor * 4;
            LocalHeatCapacityMatrix[1, 2] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[1, 3] = heatCapacityFactor * -2;
            LocalHeatCapacityMatrix[1, 4] = heatCapacityFactor * -1;

            LocalHeatCapacityMatrix[2, 1] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[2, 2] = heatCapacityFactor * 4;
            LocalHeatCapacityMatrix[2, 3] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[2, 4] = heatCapacityFactor  * -2;

            LocalHeatCapacityMatrix[3, 1] = heatCapacityFactor * -2;
            LocalHeatCapacityMatrix[3, 2] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[3, 3] = heatCapacityFactor * 4;
            LocalHeatCapacityMatrix[3, 4] = heatCapacityFactor * -1;

            LocalHeatCapacityMatrix[4, 1] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[4, 2] = heatCapacityFactor * -2;
            LocalHeatCapacityMatrix[4, 3] = heatCapacityFactor * -1;
            LocalHeatCapacityMatrix[4, 4] = heatCapacityFactor * 4;

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
            var MGlobal = new decimal[domainParams.NumberOfNodes + 1, domainParams.NumberOfNodes + 1];
            for (int elementId = 1; elementId <= domainParams.NumberOfElements; elementId++)
            {
                for (int firstNodeLocalId = 1; firstNodeLocalId <= 4; firstNodeLocalId++)
                {
                    var firstNodeGlobalId = nodeMap.GetNodeByLocalAddress(elementId, firstNodeLocalId).GlobalId;

                    for (int secondNodeLocalId = 1; secondNodeLocalId <= 4; secondNodeLocalId++)
                    {
                        var secondNodeGlobalId = nodeMap.GetNodeByLocalAddress(elementId, secondNodeLocalId).GlobalId;

                        if (secondNodeGlobalId > 0)
                            MGlobal[firstNodeGlobalId, secondNodeGlobalId] = MGlobal[firstNodeGlobalId, secondNodeGlobalId] + LocalHeatCapacityMatrix[firstNodeLocalId, secondNodeLocalId];
                    }
                }
            }

            return MGlobal;
        }
    }
}
