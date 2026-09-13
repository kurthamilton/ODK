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

    public required IReadOnlyCollection<string> ReservedSlugs { get; init; }
}