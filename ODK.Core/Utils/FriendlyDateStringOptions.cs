namespace ODK.Core.Utils;

public class FriendlyDateStringOptions
{
    public bool ForceIncludeTime { get; init; }

    public bool ForceIncludeYear { get; init; }

    public bool FullMonthName { get; init; }

    public bool IncludeDayOfWeek { get; init; }

    public bool IncludeTime { get; init; }

    public TimeZoneInfo? TimeZone { get; init; }
}