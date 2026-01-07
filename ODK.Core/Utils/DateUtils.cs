namespace ODK.Core.Utils;

public static class DateUtils
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    
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
        string format = $"ddd {(@long ? "MMMM" : "MMM")} d";

        if (includeYear)
        {
            format += " yyyy";
        }

        return date.ToString(format);
    }

    public static DateTime FromUnixEpochTimestamp(long unixTimestamp)
    {
        return UnixEpoch
            .AddSeconds(unixTimestamp)
            .ToUniversalTime();
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

    public static string ToFriendlyDateString(this DateTime dateUtc, TimeZoneInfo? timeZone, bool forceIncludeYear = false)
    {
        var localDate = timeZone != null 
            ? TimeZoneInfo.ConvertTimeFromUtc(dateUtc, timeZone)
            : dateUtc;

        var includeYear = forceIncludeYear || dateUtc.Year != DateTime.UtcNow.Year;
        
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

    public static long ToUnixEpochTimestamp(DateTime dateTime)
    {
        var diff = dateTime.ToUniversalTime() - UnixEpoch;
        return (long)Math.Floor(diff.TotalSeconds);
    }

    public static DateTime ToUtc(this DateTime local, TimeZoneInfo timeZone) 
        => TimeZoneInfo.ConvertTimeToUtc(local.SpecifyKind(DateTimeKind.Unspecified), timeZone);
}
