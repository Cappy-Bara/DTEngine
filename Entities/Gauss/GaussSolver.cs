namespace DTEngine.Entities.Gauss
{
    public static class GaussSolver
    {
        public static decimal[] Solve(int nw, AMatrix a)
        {
            var gaussResult = new decimal[251];
            int spr = 1;
            decimal max, zamiana, stala, b;


            int iz = -1;
            while (iz < nw - 1 && spr != 0)
            {
                iz++;
                max = a[iz, iz] * a[iz, iz];
                int izmax = iz;
                for (int j = iz + 1; j < nw; j++)
                {
                    if (max < a[j, iz] * a[j, iz])
                    {
                        max = a[j, iz] * a[j, iz];
                        izmax = j;
                    }
                }
                if (izmax != iz)
                {
                    for (int j = 0; j < nw + 1; j++)
                    {
                        zamiana = a[izmax, j];
                        a[izmax, j] = a[iz, j];
                        a[iz, j] = zamiana;
                    }
                }
                for (int j = iz + 1; j < nw; j++)
                {
                    if (a[iz, iz] == 0)
                        spr = 0;

                    stala = a[j, iz] / a[iz, iz];
                    for (int k = iz; k < nw + 1; k++)
                        a[j, k] = a[j, k] - stala * a[iz, k];
                }
            }
            for (iz = nw - 1; iz > -1; iz--)
            {
                b = 0;
                for (int j = nw - 1; j > iz; j--)
                    b += a[iz, j] * gaussResult[j];

                gaussResult[iz] = (a[iz, nw] - b) / a[iz, iz];
                if (a[iz, iz] == 0)
                    spr = 0;
            }

            if (spr == 0)
            {
                throw new Exception("Gauss method found no solutions for equation.");
            }

            return gaussResult;
        }
    }
}
