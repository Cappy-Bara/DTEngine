using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Helpers
{
    public static class Dumper
    {
        public static T[,] Dump<T>(this T[,] matrix, string name, int fromIndex, int size)
        {
#if VERBOSE
            Console.WriteLine($"\n{name}");

            for (int i = fromIndex; i < size; i++)
            {
                for (int j = fromIndex; j < size; j++)
                {
                    Console.Write($"\t{matrix[i, j]} ");
                }
                Console.WriteLine();
            }
#endif
            return matrix;
        }


        public static T[,] DumpVector<T>(this T[,] vector, string name, int columnId, int fromIndex, int size)
        {
#if VERBOSE
            Console.WriteLine($"\n{name}");

            for (int i = fromIndex; i < size; i++)
            {
                 Console.WriteLine($"{vector[i, columnId]}");
            }
#endif
            return vector;
        }
    }
}



