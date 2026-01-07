namespace ODK.Core.Chapters;

public class ChapterPaymentAccount : IDatabaseEntity, IChapterEntity
{
    public DateTime? CardPaymentsEnabledUtc { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public required string ExternalId { get; set; }

    public Guid Id { get; set; }

    public DateTime? IdentityDocumentsProvidedUtc { get; set; }

    public DateTime? OnboardingCompletedUtc { get; set; }

    public required string? OnboardingUrl { get; set; }

    public Guid OwnerId { get; set; }

    public Guid SitePaymentSettingId { get; set; }

    public bool SetupComplete()
    {
        return
            OnboardingCompletedUtc != null &&
            IdentityDocumentsProvidedUtc != null;
    }
}
