using DTEngine.Utilities;
using Microsoft.Win32;
using System.Text;

namespace DTEngine.Helpers
{
    public static class MatrixInverter
    {
        public static decimal[,] Invert(decimal[,] macierz, int roz)
        {
            var macierzOdw = new decimal[roz, roz];

            int i, j, k;
            decimal s;
            decimal[] p_dTmp1 = new decimal[roz];
            decimal[] p_dTmp2 = new decimal[roz];
            for (j = 0; j < roz; j++)
            {
                for (int z = 0; z < roz; z++)
                {
                    p_dTmp1[z] = macierzOdw[j, z];
                }

                for (i = 0; i < roz; i++)
                    p_dTmp1[i] = 0.0m;
                p_dTmp1[j] = 1.0m;
            }
            for (i = 0; i < roz; i++)
            {

                for (int z = 0; z < roz; z++)
                {
                    p_dTmp1[z] = macierz[i, z];
                    p_dTmp2[z] = macierzOdw[i, z];
                }

                s = 1.0m / p_dTmp1[i];
                for (j = 0; j < roz; j++)
                {
                    p_dTmp1[j] *= s;
                    p_dTmp2[j] *= s;
                }
                for (j = i + 1; j < roz; j++)
                {
                    s = macierz[j, i];

                    for (int z = 0; z < roz; z++)
                    {
                        p_dTmp1[z] = macierz[j, z];
                        p_dTmp2[z] = macierz[i, z];
                    }

                    for (k = i; k < roz; k++)
                        p_dTmp1[k] -= p_dTmp2[k] * s;

                    for (int z = 0; z < roz; z++)
                    {
                        p_dTmp1[z] = macierzOdw[j, z];
                        p_dTmp2[z] = macierzOdw[i, z];
                    }

                    for (k = 0; k < roz; k++)
                        p_dTmp1[k] -= p_dTmp2[k] * s;
                }
            }

            for (k = 0; k < roz; k++)
            {
                for (i = roz - 1; i >= 0; i--)
                {
                    s = macierzOdw[i, k];

                    for (int z = 0; z < roz; z++)
                    {
                        p_dTmp1[z] = macierz[i, z];
                    }

                    for (j = i + 1; j < roz; j++)
                        s -= p_dTmp1[j] * macierzOdw[j, k];
                    macierzOdw[i, k] = s;
                }
            }

            return macierzOdw;
        }
    }
}
