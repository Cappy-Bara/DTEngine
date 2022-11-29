using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTEngine.Entities.FVector.FAlpha
{
    public interface IFAlphaFactory
    {
        public decimal[,] Generate(decimal temperatureTop, decimal temperatureBottom,
            int[] clipsUp, int[] clipsDown, decimal heatExchangingFactor1, decimal heatExchangingFactor2);
    }
}
