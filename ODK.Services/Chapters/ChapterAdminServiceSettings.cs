namespace ODK.Services.Chapters;

public class ChapterAdminServiceSettings
{
    public required double ContactMessageRecaptchaScoreThreshold { get; init; }

    public required string DefaultCountryCode { get; init; }

    public required IReadOnlyCollection<string> ReservedSlugs { get; init; }
}