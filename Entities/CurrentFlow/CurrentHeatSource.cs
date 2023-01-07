using DTEngine.Entities.ComputingDomain;

namespace DTEngine.Entities.CurrentFlow
{
    public class CurrentHeatSource
    {
        private ComputationalDomainParams domainParams;
        public CurrentHeatSource(ComputationalDomainParams domainParams)
        {
            this.domainParams = domainParams;
        }

        public decimal[,] GetHeatSourceValues(decimal time, decimal[,] temperatures)
        {
            var output = new decimal[domainParams.NumberOfNodes+1, 2];

            var PA = 2400000m;

            for (int j = 1; j <= domainParams.NumberOfNodes; j++)
            {
                var r = ResistanceApproximator.GetResistance(temperatures[j, 1]);
                var i = Current.GetCurrentValue(time);

                output[j, 1] = i * i * r * PA;
            }

            return output;
        }
    }
}
