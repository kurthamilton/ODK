using System;

namespace ODK.Core.Utils
{
    public static class DateUtils
    {
        public static long DateVersion(DateTime date)
        {
            return long.Parse($"{date:yyyyMMdd}");
        }

        public static string EventDate(this DateTime date, bool @long = false)
        {
            bool includeYear = date.Year > DateTime.UtcNow.Year;
            string format = $"ddd {(@long ? "MMMM" : "MMM")} d";
            if (includeYear)
            {
                format += " yyyy";
            }

            return date.ToString(format);
        }
    }
}
