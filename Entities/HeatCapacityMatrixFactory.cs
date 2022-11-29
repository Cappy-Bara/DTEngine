using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;
using System.Globalization;

namespace DTEngine.Entities
{
    public class HeatCapacityMatrixFactory
    {
        public decimal[,] LocalHeatCapacityMatrix { get; init; } = new decimal[5, 5];
        public decimal[,] GlobalHeatCapacityMatrix { get => GetGlobalMatrix(); }

        private ComputationalDomainParams domainParams;
        private NodeMap nodeMap;

        private decimal[,]? globalStiffnessMatrix;

        public HeatCapacityMatrixFactory(decimal heatCapacity, decimal density, ComputationalDomainParams domainParams, NodeMap nodeMap)
        {
            var baseHeatCapacityMatrix = new decimal[,] { { 0, 0, 0, 0, 0 }, { 0, 4, 2, 1, 2 }, { 0, 2, 4, 2, 1 }, { 0, 1, 2, 4, 2 }, { 0, 2, 1, 2, 4 } };
            baseHeatCapacityMatrix.Dump("BASE HEAT CAPACITY MATRIX:", 1,5);

             var heatCapacityFactor = heatCapacity * density * domainParams.HeightStep * domainParams.WidthStep / 36;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    LocalHeatCapacityMatrix[i+1, j+1] = baseHeatCapacityMatrix[i + 1, j + 1] * heatCapacityFactor;
                }
            }

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

            return MGlobal.Dump("HEAT CAPACITY MATRIX:", 1, domainParams.NumberOfNodes + 1);
        }
    }
}
