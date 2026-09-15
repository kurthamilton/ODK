namespace ODK.Services.Chapters;

public class ChapterAdminServiceSettings
{
    public required double ContactMessageRecaptchaScoreThreshold { get; init; }

    /// <summary>
    /// How much of the group the admin dashboard shows at a glance. Both are a row of cards wide, so the
    /// numbers are what fits rather than what is available.
    /// </summary>
    public required int DashboardNewestMemberCount { get; init; }

    /// <inheritdoc cref="DashboardNewestMemberCount"/>
    public required int DashboardUpcomingEventCount { get; init; }

    public required string DefaultCountryCode { get; init; }

    /// <summary>
    /// How long a group counts as newly arrived (Groups:MigrationWindowDays) - here, how long its
    /// dashboard keeps offering it the words to tell its old community with.
    /// </summary>
    public required int MigrationWindowDays { get; init; }

    public required IReadOnlyCollection<string> ReservedSlugs { get; init; }
}