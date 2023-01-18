using DTEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities
{
    public class AlphaMatixes
    {
        public decimal[,] alfa12;
        public decimal[,] alfa23;
        public decimal[,] alfa34;
        public decimal[,] alfa41;

        public AlphaMatixes()
        {
            alfa12 = new decimal[5, 5];
            alfa12[1, 1] = 2;
            alfa12[1, 2] = 1;
            alfa12[2, 2] = 2;
            alfa12[2, 1] = 1;
            alfa12.Dump("ALFA 12:", 1, 5);

            alfa23 = new decimal[5, 5];
            alfa23[2, 2] = 2;
            alfa23[2, 3] = 1;
            alfa23[3, 2] = 1;
            alfa23[3, 3] = 2;
            alfa23.Dump("ALFA 23:", 1, 5);

            alfa34 = new decimal[5, 5];
            alfa34[3, 3] = 2;
            alfa34[3, 4] = 1;
            alfa34[4, 3] = 1;
            alfa34[4, 4] = 2;
            alfa34.Dump("ALFA 34:", 1, 5);

            alfa41 = new decimal[5, 5];
            alfa41[1, 1] = 2;
            alfa41[1, 4] = 1;
            alfa41[4, 1] = 1;
            alfa41[4, 4] = 2;
            alfa41.Dump("ALFA 41:", 1, 5);
        }
    }
}
