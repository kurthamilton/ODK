namespace ODK.Core.Chapters;

public class ChapterPaymentAccount : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public required string ExternalId { get; set; }

    public Guid Id { get; set; }

    public DateTime? OnboardingCompletedUtc { get; set; }

    public required string? OnboardingUrl { get; set; }
}
