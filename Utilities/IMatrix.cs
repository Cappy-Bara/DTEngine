using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Utilities
{
    public interface IMatrix<T>
    {
        T this[int index, int index2]
        {
            get;
        }
    }
}
