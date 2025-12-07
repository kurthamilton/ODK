using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Core.Chapters;

public class ChapterSubscription : IDatabaseEntity, IChapterEntity, IChapterPaymentEntity
{
    public double Amount { get; set; }

    public Guid ChapterId { get; set; }

    public string Description { get; set; } = "";

    public bool Disabled { get; set; }

    public string? ExternalAccountId { get; set; }

    public string? ExternalId { get; set; }

    public string? ExternalProductId { get; set; }

    public Guid Id { get; set; }

    public int Months { get; set; }

    public string Name { get; set; } = "";

    public bool Recurring { get; set; }

    public SitePaymentSettings? SitePaymentSettings { get; set; }

    public Guid? SitePaymentSettingId { get; set; }

    public string Title { get; set; } = "";

    public SubscriptionType Type { get; set; }

    public bool Enabled()
    {
        if (Disabled)
        {
            return false; 
        }

        return SitePaymentSettings == null || SitePaymentSettings.Active;
    }

    public string ToReference() => $"Subscription: {Name}";

    public bool Uses(ChapterPaymentSettings? chapterPaymentSettings, SitePaymentSettings sitePaymentSettings)
    {
        return chapterPaymentSettings?.UseSitePaymentProvider == true
            ? SitePaymentSettingId == sitePaymentSettings.Id
            : SitePaymentSettingId == null;
    }
}
