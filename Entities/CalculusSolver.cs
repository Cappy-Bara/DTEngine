using DTEngine.Entities.ComputingDomain;

namespace DTEngine.Entities
{
    public class CalculusSolver
    {
        private readonly ComputationalDomainParams domainParams;
        private readonly decimal[,] invertedHeatMatrix;
        private readonly decimal[,] KSum;
        private readonly decimal[,] f;

        public CalculusSolver(ComputationalDomainParams domainParams, decimal[,] invertedHeatMatrix, decimal[,] KSum, decimal[,] f, decimal[,] t0)
        {
            this.domainParams = domainParams;
            this.invertedHeatMatrix = invertedHeatMatrix;
            this.KSum = KSum;
            this.f = f;
            Result = t0;
        }

        public decimal[,] Result { get; set; }


        public void SolveStep(decimal dt)
        {
            var m1 = new decimal[64, 2];
            var m2 = new decimal[64, 2];
            var m3 = new decimal[64, 2];
            var m4 = new decimal[64, 2];
            var m5 = new decimal[64, 2];

            //1 operation - (m-1*f)
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    m1[k, j] = 0;
                    for (int i = 1; i <= domainParams.NumberOfNodes; i++)
                    {
                        m1[k, j] = m1[k, j] + (invertedHeatMatrix[k, i] * f[i, j]);
                    }
                }
            }
            //2 operation
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    m2[k, j] = 0;
                    for (int i = 1; i <= domainParams.NumberOfNodes; i++)
                    {
                        m2[k, j] = m2[k, j] + (KSum[k, i] * Result[i, j]);
                    }
                }
            }
            //3 operation
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    m3[k, j] = 0;
                    for (int i = 1; i <= domainParams.NumberOfNodes; i++)
                    {
                        m3[k, j] = m3[k, j] + (invertedHeatMatrix[k, i] * m2[i, j]);
                    }
                }
            }
            //4 operation
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    m4[k, j] = m1[k, j] - m3[k, j];
                }
            }
            //5 operation
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    m5[k, j] = m4[k, j] * dt;
                }
            }
            //6 operation
            for (int k = 1; k <= domainParams.NumberOfNodes; k++)
            {
                for (int j = 1; j <= 1; j++)
                {
                    Result[k, j] = Result[k, j] + m5[k, j];
                }
            }
        }

        public void PrintStep(int sec, int step)
        {
            Console.WriteLine($"\nTime vector    -   {sec}sec   -> step {step}");
            for (int j = 1; j <= domainParams.HorizontalElementsQuantity; j++)
            {
                int firstElement = j;
                int secondElement = j + domainParams.HorizontalElementsQuantity;
                int thirdElement = j + 2 * domainParams.HorizontalElementsQuantity;

                Console.WriteLine($"{firstElement:D2}={Result[firstElement, 1]:F4}\t" +
                    $"{secondElement:D2}={Result[secondElement, 1]:F4}\t" +
                    $"{thirdElement:D2}={Result[thirdElement, 1]:F4}\t");
            }
        }
    }
}
