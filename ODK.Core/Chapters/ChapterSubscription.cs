using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Core.Chapters;

public class ChapterSubscription : IDatabaseEntity, IChapterEntity
{
    public double Amount { get; set; }

    public Guid ChapterId { get; set; }

    public string Description { get; set; } = "";

    public bool Disabled { get; set; }

    public string? ExternalId { get; set; }

    public string? ExternalProductId { get; set; }

    public Guid Id { get; set; }

    public int Months { get; set; }

    public string Name { get; set; } = "";

    public Guid? SitePaymentSettingId { get; set; }

    public string Title { get; set; } = "";

    public SubscriptionType Type { get; set; }

    public bool Use(ChapterPaymentSettings chapterPaymentSettings, SitePaymentSettings sitePaymentSettings)
    {
        return chapterPaymentSettings.UseSitePaymentProvider
            ? SitePaymentSettingId == sitePaymentSettings.Id
            : SitePaymentSettingId == null;
    }
}
