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

    public static string EventDate(this DateTime date, bool @long = false, DayOfWeek defaultDayOfWeek = DayOfWeek.Wednesday)
    {
        bool includeYear = date.Year != DateTime.UtcNow.Year;
        bool includeDayOfWeek = date.DayOfWeek != defaultDayOfWeek;
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

    public static DateTime SpecifyKind(this DateTime date, DateTimeKind kind) => DateTime.SpecifyKind(date, kind);

    public static DateTime? SpecifyKind(this DateTime? date, DateTimeKind kind) 
        => date != null ? date.Value.SpecifyKind(kind) : new DateTime?();

    public static DateTime StartOfDay(this DateTime date) => date - date.TimeOfDay;

    public static string ToFriendlyDateString(this DateTime dateUtc, TimeZoneInfo? timeZone)
    {
        var localDate = timeZone != null 
            ? TimeZoneInfo.ConvertTimeFromUtc(dateUtc, timeZone)
            : dateUtc;

        var includeYear = dateUtc.Year != DateTime.UtcNow.Year;
        
        var format = "ddd, MMM d";

        if (includeYear)
        {
            format += ", yyyy";
        }

        return localDate.ToString(format);
    }

    public static string ToFriendlyDateTimeString(this DateTime dateUtc, TimeZoneInfo? timeZone)
    {
        var localDate = timeZone != null
            ? TimeZoneInfo.ConvertTimeFromUtc(dateUtc, timeZone)
            : dateUtc;

        var includeYear = dateUtc.Year != DateTime.UtcNow.Year;

        var format = "ddd, MMM d";

        if (includeYear)
        {
            format += ", yyyy";
        }

        format += " HH:mm";

        return localDate.ToString(format);
    }

    public static DateTime ToUtc(this DateTime local, TimeZoneInfo timeZone) 
        => TimeZoneInfo.ConvertTimeToUtc(local.SpecifyKind(DateTimeKind.Unspecified), timeZone);
}
