namespace DTEngine.Entities
{
    public class StiffnessMatrix
    {
        public decimal[,] LocalStiffnessMatrix { get; init; } = new decimal[5, 5];
        public decimal[,] GlobalStiffnessMatrix { get => GetGlobalMatrix(); }

        private decimal[,]? globalStiffnessMatrix;
        private decimal numberOfElements;
        private int[,] connectionMatrix;



        public StiffnessMatrix(decimal conductingFactor, int[,] connectionMatrix, decimal numberOfElements)
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

            this.connectionMatrix = connectionMatrix;
            this.numberOfElements = numberOfElements;

            globalStiffnessMatrix = null;
        }

        private decimal[,] GetGlobalMatrix()
        {
            globalStiffnessMatrix ??= GenerateGlobalMatrix();
         
            return globalStiffnessMatrix;
        }

        private decimal[,] GenerateGlobalMatrix()
        {
            //building global K matrix
            var KGlobal = new decimal[10, 10];
            for (int element = 1; element <= numberOfElements; element++)
            {
                for (int firstNodeLocal = 1; firstNodeLocal <= 4; firstNodeLocal++)
                {
                    var firstNodeGlobal = connectionMatrix[element, firstNodeLocal];
                    for (int secondNodeLocal = 1; secondNodeLocal <= 4; secondNodeLocal++)
                    {
                        var secondNodeGlobal = connectionMatrix[element, secondNodeLocal];

                        if (secondNodeGlobal > 0)
                            KGlobal[firstNodeGlobal, secondNodeGlobal] = KGlobal[firstNodeGlobal, secondNodeGlobal] + LocalStiffnessMatrix[firstNodeLocal, secondNodeLocal];
                    }
                }
            }

            return KGlobal;
        }
    }
}
