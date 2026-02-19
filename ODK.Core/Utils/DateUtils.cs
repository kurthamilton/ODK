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

        if (dateUtc.TimeOfDay.Ticks > 0)
        {
            format += " HH:mm";
        }

        return localDate.ToString(format);
    }

    /// <summary>
    /// Returns a human-readable relative time string.
    /// For recent times (under 24h), uses elapsed language ("5 minutes ago").
    /// For older times, uses calendar language ("yesterday", "3 days ago")
    /// resolved against the user's local timezone where day boundaries matter.
    /// </summary>
    /// <param name="dateUtc">The UTC timestamp to describe.</param>
    /// <param name="timeZone">
    /// The user's timezone, used to resolve calendar boundaries (yesterday, etc.).
    /// Defaults to UTC if null.
    /// </param>
    public static string ToRelativeTime(DateTime dateUtc, TimeZoneInfo? timeZone = null)
    {
        timeZone ??= TimeZoneInfo.Utc;

        var utcNow = DateTime.UtcNow;
        var elapsed = utcNow - dateUtc;

        // Future timestamps — clock skew, optimistic saves, etc.
        if (elapsed.TotalSeconds < 0)
        {
            return "just now";
        }

        // Under a minute
        if (elapsed.TotalSeconds < 60)
        {
            return "just now";
        }

        // Under an hour
        if (elapsed.TotalHours < 1)
        {
            var minutes = (int)elapsed.TotalMinutes;
            return $"{minutes} {StringUtils.Pluralise(minutes, "minute")} ago";
        }

        // Under 24 hours — still elapsed language, no timezone needed
        if (elapsed.TotalHours < 24)
        {
            var hours = (int)elapsed.TotalHours;
            return $"{hours} {StringUtils.Pluralise(hours, "hour")} ago";
        }

        // 24h+ — switch to calendar language, timezone matters here
        var userNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        var userTimestamp = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, timeZone);

        var calendarDaysAgo = (int)(userNow.Date - userTimestamp.Date).TotalDays;

        if (calendarDaysAgo == 1)
        {
            return "yesterday";
        }

        if (calendarDaysAgo < 7)
        {
            return $"{calendarDaysAgo} days ago";
        }

        if (calendarDaysAgo < 14)
        {
            return "last week";
        }

        if (calendarDaysAgo < 30)
        {
            var weeks = calendarDaysAgo / 7;
            return $"{weeks} {StringUtils.Pluralise(weeks, "week")} ago";
        }

        if (calendarDaysAgo < 60)
        {
            return "last month";
        }

        if (calendarDaysAgo < 365)
        {
            var months = calendarDaysAgo / 30;
            return $"{months} {StringUtils.Pluralise(months, "month")} ago";
        }

        if (calendarDaysAgo < 730)
        {
            return "last year";
        }

        var years = calendarDaysAgo / 365;
        return $"{years} {StringUtils.Pluralise(years, "year")} ago";
    }

    public static long ToUnixEpochTimestamp(DateTime dateTime)
    {
        var diff = dateTime.ToUniversalTime() - UnixEpoch;
        return (long)Math.Floor(diff.TotalSeconds);
    }

    public static DateTime ToUtc(this DateTime local, TimeZoneInfo timeZone)
        => TimeZoneInfo.ConvertTimeToUtc(local.SpecifyKind(DateTimeKind.Unspecified), timeZone);
}