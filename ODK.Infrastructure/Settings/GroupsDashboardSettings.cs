namespace ODK.Infrastructure.Settings;

/// <summary>
/// How much of a group the admin dashboard shows at a glance. Both are a row of cards wide, so the
/// numbers are what fits rather than what is available.
/// </summary>
public class GroupsDashboardSettings
{
    public required int NewestMemberCount { get; init; }

    public required int UpcomingEventCount { get; init; }
}
