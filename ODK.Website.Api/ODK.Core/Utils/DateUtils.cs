using System;

namespace ODK.Core.Utils
{
    public static class DateUtils
    {
        public static long DateVersion(DateTime date)
        {
            return long.Parse($"{date}:yyyyMMdd");
        }
    }
}
