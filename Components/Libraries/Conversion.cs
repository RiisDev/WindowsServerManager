namespace WindowsServerManager.Components.Libraries
{
    public static class Conversion
    {
        public static string ConvertSecondsToTime(int seconds)
        {
            // Define the conversion values
            List<(string Name, int Value)> units =
            [
                ("month", 30 * 24 * 60 * 60), // 1 month = 30 days
                ("week", 7 * 24 * 60 * 60), // 1 week = 7 days
                ("day", 24 * 60 * 60), // 1 day = 24 hours
                ("hour", 60 * 60), // 1 hour = 60 minutes
                ("minute", 60)
            ];

            List<string> result = [];

            // Calculate each unit of time
            foreach ((string name, int value) in units)
            {
                if (seconds < value) continue;

                int count = Math.DivRem(seconds, value, out seconds);
                result.Add(count > 1 ? $"{count} {name}s" : $"{count} {name}");
            }

            return string.Join(", ", result);
        }
    }
}
