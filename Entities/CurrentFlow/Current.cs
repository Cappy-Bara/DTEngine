namespace DTEngine.Entities.CurrentFlow
{
    public static class Current
    {
        private static decimal currentValue;

        public static void InitializeCurrent(decimal current)
        {
            currentValue = current;
        }


        public static decimal GetCurrentValue(decimal time)
        {
            return currentValue;
        }
    }
}
