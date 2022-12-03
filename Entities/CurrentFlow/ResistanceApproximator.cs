namespace DTEngine.Entities.CurrentFlow
{
    public static class ResistanceApproximator
    {
        private readonly static Dictionary<int, decimal> resistanceValues = new() { 
            {25,0.14341m },
            {80,0.17808m },
            {120,0.20505m },
            {200,0.26323m },
            {300,0.34353m },
            {420,0.44923m },
            {520,0.5478m },
            {600,0.62394m },
            {700,0.73238m },
            {720,0.77952m },
            {820,0.98473m },
            {880,1.01833m },
            {900,1.02669m },
            {1120,1.13619m },
            {1240,1.14885m },
            {1280,1.16112m },
            {1400,1.19571m },
            {1460,1.21183m },
            {1465,1.21341m },
            {1480,1.22334m },
            {1483,1.2285m },
            {1486,1.16187m},
            {1500,1.20672m },
            {1513,1.31337m},
            {1520,1.31344m },
        };
        private readonly static decimal multiplier = 0.000001m;

        public static decimal GetResistance(decimal temperature)
        {
            if (temperature < 25)
                return 0.14341m* multiplier;

            if (temperature > 1520)
                return 1.31344m* multiplier;

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
