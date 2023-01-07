namespace DTEngine.Entities.CurrentFlow
{
    public static class ResistanceApproximator
    {
        private readonly static Dictionary<int, decimal> resistanceValues = new() { 
            {0,0.000255194m },
            {20,0.000255194m },
            {100,0.000297929m},
            {200,0.000342882m },
            {300,0.00037817m },
            {400,0.000406698m },
            {500,0.00043034m },
            {600,0.000450338m },
            {700,0.000483815m },
            {800,0.000507251m },
            {900,0.000518337m },
            {1000,0.000527119m },
            {1200,0.000527119m},
        };
        //private readonly static decimal multiplier = 0.000001m;
        private readonly static decimal multiplier = 1m;

        public static decimal GetResistance(decimal temperature)
        {
            if (temperature < 0)
                return 0.000255194m * multiplier;

            if (temperature > 1200)
                return 0.000527119m * multiplier;

            int parsedTemp = (int)temperature;
            bool success = resistanceValues.TryGetValue((int)temperature, out var resistance);
            
            if(success)
                return resistance * multiplier;

            var Tmin = resistanceValues.Keys.Where(x => x < parsedTemp).Max();
            var Tmax = resistanceValues.Keys.Where(x => x > parsedTemp).Min();
            var Rmin = resistanceValues[Tmin];
            var Rmax = resistanceValues[Tmax];

            var output = ((Rmin - Rmax) / (Tmin - Tmax)) * temperature + (Rmin - ((Rmin-Rmax)/(Tmin-Tmax))*Tmin);
            return output * multiplier;
        }
    }
}
