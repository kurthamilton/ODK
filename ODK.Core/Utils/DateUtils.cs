namespace ODK.Core.Utils;

public static class DateUtils
{
    public static long DateVersion(DateTime date)
    {
        return long.Parse($"{date:yyyyMMdd}");
    }

    public static IEnumerable<DayOfWeek> DaysOfWeek(DayOfWeek firstDayOfWeek)
    {
        return Enum
            .GetValues<DayOfWeek>()
            .OrderBy(day => day < firstDayOfWeek);
    }

    public static string EventDate(this DateTime date, bool @long = false)
    {
        bool includeYear = date.Year != DateTime.UtcNow.Year;
        bool includeDayOfWeek = date.DayOfWeek != DayOfWeek.Wednesday;
        string format = "";
        if (includeDayOfWeek)
        {
            format = "ddd ";
        }

        format += $"{(@long ? "MMMM" : "MMM")} d";

        if (includeYear)
        {
            format += " yyyy";
        }

        return date.ToString(format);
    }

    public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
    {
        date = date.AddDays(1);
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    public static DateTime Previous(this DateTime date, DayOfWeek dayOfWeek)
    {
        date = date.AddDays(-1);
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(-1);
        }

        return date;
    }
}
