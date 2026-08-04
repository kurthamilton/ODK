using System.Globalization;

namespace ODK.Core.Utils;

public record FriendlyDateStringOptions
{
    /// <summary>
    /// The culture whose day/month order and month/day names are used. Defaults to
    /// <see cref="CultureInfo.CurrentCulture"/> (the request culture in a web request). Request-independent
    /// callers - emails, notifications, exports - must set this explicitly (see
    /// <see cref="LocaleUtils.DefaultCulture"/>) so the text never inherits the ambient request culture.
    /// </summary>
    public CultureInfo? Culture { get; init; }

    public bool ForceIncludeTime { get; init; }

    public bool ForceIncludeYear { get; init; }

    public bool FullMonthName { get; init; }

    public bool IncludeDayOfWeek { get; init; }

    public bool IncludeTime { get; init; }

    public TimeZoneInfo? TimeZone { get; init; }
}