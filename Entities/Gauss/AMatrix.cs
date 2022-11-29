using DTEngine.Entities.ComputingDomain;
using DTEngine.Helpers;
using DTEngine.Utilities;
using System;

namespace DTEngine.Entities.Gauss
{
    public class AMatrix : IMatrix<decimal>
    {
        private decimal[,] a = new decimal[201, 201];

        public AMatrix(Dictionary<int, decimal> initValues, decimal[,] KSum, decimal[,] f, ComputationalDomainParams domainParams)
        {
            for (int row = 0; row < domainParams.NumberOfNodes; row++)
            {
                for (int column = 0; column < domainParams.NumberOfNodes; column++)
                {
                    if (row == column && initValues.TryGetValue(row + 1, out _))
                    {
                        a[row, column] = 1;
                    }
                    else if (initValues.TryGetValue(row + 1, out _))
                    {
                        a[row, column] = 0;
                    }
                    else if (initValues.TryGetValue(column + 1, out var initVal))
                    {
                        var aVal = KSum[row + 1, column + 1] * initVal;
                        f[row + 1, 1] -= aVal;
                        a[row, column] = 0;
                    }
                    else
                    {
                        a[row, column] = a[row, column] + KSum[row + 1, column + 1];
                    }
                }

                var isInitValue = initValues.TryGetValue(row + 1, out var value);
                a[row, domainParams.NumberOfNodes] = isInitValue ? value : f[row + 1, 1];
            }

            a.Dump("A MATRIX", 1, domainParams.NumberOfNodes);
        }

        public decimal this[int index, int index2]
        {
            get => a[index, index2];
            set => a[index, index2] = value;
        }
    }
}
