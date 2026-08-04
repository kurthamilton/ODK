using System.Globalization;

namespace ODK.Core.Utils;

public static class DateUtils
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// A short UTC-offset label (e.g. "(UTC+1)") for <paramref name="chapterTimeZone"/> at
    /// <paramref name="dateUtc"/> - DST-aware - shown to a viewer in a different timezone so an event's
    /// wall-clock time isn't misread as their own local time. Empty when the viewer's timezone is unknown
    /// (<paramref name="memberTimeZone"/> is null) or the same as the chapter's.
    /// </summary>
    public static string ChapterTimeZoneLabel(
        TimeZoneInfo chapterTimeZone, TimeZoneInfo? memberTimeZone, DateTime dateUtc)
    {
        if (memberTimeZone == null || memberTimeZone.Id == chapterTimeZone.Id)
        {
            return string.Empty;
        }

        var offset = chapterTimeZone.GetUtcOffset(DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc));
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var magnitude = offset.Duration();
        var hoursMinutes = magnitude.Minutes == 0
            ? magnitude.Hours.ToString(CultureInfo.InvariantCulture)
            : $"{magnitude.Hours}:{magnitude.Minutes:D2}";

        return $"(UTC{sign}{hoursMinutes})";
    }

    public static long DateVersion(DateTime date) => long.Parse($"{date:yyyyMMdd}");

    public static IEnumerable<DayOfWeek> DaysOfWeek(DayOfWeek firstDayOfWeek)
        => Enum.GetValues<DayOfWeek>()
            .OrderBy(day => day < firstDayOfWeek);

    public static string EventDate(this DateTime date, TimeZoneInfo timeZone)
        => date.ToFriendlyDateString(options: new FriendlyDateStringOptions
        {
            TimeZone = timeZone
        });

    public static DateTime FromUnixEpochTimestamp(long unixTimestamp)
        => UnixEpoch
            .AddSeconds(unixTimestamp)
            .ToUniversalTime();

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

    public static DateTime SpecifyKind(this DateTime date, DateTimeKind kind)
        => DateTime.SpecifyKind(date, kind);

    public static DateTime? SpecifyKind(this DateTime? date, DateTimeKind kind)
        => date != null ? date.Value.SpecifyKind(kind) : new DateTime?();

    public static DateTime StartOfDay(this DateTime date) => date - date.TimeOfDay;

    public static string ToFriendlyDateString(this DateTime dateUtc, FriendlyDateStringOptions? options)
    {
        var timeZone = options?.TimeZone;
        var culture = options?.Culture ?? CultureInfo.CurrentCulture;

        var localDate = timeZone != null
            ? TimeZoneInfo.ConvertTimeFromUtc(dateUtc, timeZone)
            : dateUtc;

        var includeYear = options?.ForceIncludeYear == true || dateUtc.Year != DateTime.UtcNow.Year;
        var monthToken = options?.FullMonthName == true ? "MMMM" : "MMM";

        var format = options?.IncludeDayOfWeek == true
            ? "ddd, "
            : string.Empty;

        // The day/month order follows the culture ("5 Jun" for en-GB, "Jun 5" for en-US); the year, when
        // shown, is appended at the end. Year-first orderings (e.g. CJK cultures) are out of scope.
        format += DayBeforeMonth(culture)
            ? $"d {monthToken}{(includeYear ? " yyyy" : "")}"
            : $"{monthToken} d{(includeYear ? ", yyyy" : "")}";

        if (options?.ForceIncludeTime == true || (options?.IncludeTime == true && localDate.TimeOfDay.Ticks > 0))
        {
            format += " HH:mm";
        }

        return localDate.ToString(format, culture);
    }

    public static string ToFriendlyDateTimeString(this DateTime dateUtc, FriendlyDateStringOptions? options)
        => dateUtc.ToFriendlyDateString((options ?? new FriendlyDateStringOptions()) with
        {
            ForceIncludeTime = true
        });

    /// <summary>
    /// Returns a human-readable relative time string.
    /// For recent times (under 24h), uses elapsed language ("5 minutes ago").
    /// For older times, uses calendar language ("yesterday", "3 days ago")
    /// resolved against the user's local timezone where day boundaries matter.
    /// </summary>
    /// <param name="dateUtc">The UTC timestamp to describe.</param>
    /// <param name="timeZone">
    /// The user's timezone, used to resolve calendar boundaries (yesterday, etc.).
    /// </param>
    public static string ToRelativeTime(this DateTime dateUtc, TimeZoneInfo timeZone)
    {
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

    // Whether the culture writes the day before the month (e.g. en-GB "d MMMM") rather than the month
    // before the day (e.g. en-US "MMMM d"), inferred from its month/day pattern.
    private static bool DayBeforeMonth(CultureInfo culture)
    {
        var pattern = culture.DateTimeFormat.MonthDayPattern;
        var dayIndex = pattern.IndexOf('d');
        var monthIndex = pattern.IndexOf('M');
        return dayIndex >= 0 && (monthIndex < 0 || dayIndex < monthIndex);
    }
}