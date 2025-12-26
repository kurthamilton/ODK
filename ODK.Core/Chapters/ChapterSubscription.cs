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

    public bool Recurring { get; set; }

    public Guid? SitePaymentSettingId { get; set; }

    public string Title { get; set; } = "";

    public SubscriptionType Type { get; set; }

    public bool IsVisibleToMembers(
        ChapterPaymentSettings chapterPaymentSettings, IEnumerable<SitePaymentSettings> sitePaymentSettings)
    {
        if (Disabled)
        {
            return false; 
        }

        return IsVisibleToAdmins(chapterPaymentSettings, sitePaymentSettings);
    }

    public bool IsVisibleToAdmins(
        ChapterPaymentSettings chapterPaymentSettings, IEnumerable<SitePaymentSettings> sitePaymentSettings)
    {
        if (SitePaymentSettingId != null)
        {
            return sitePaymentSettings.First(x => x.Id == SitePaymentSettingId.Value).Enabled;
        }

        if (chapterPaymentSettings.UseSitePaymentProvider)
        {
            // SitePaymentSettingId not set, cannot be used
            return false;
        }

        return true;
    }



    public string ToReference() => $"Subscription: {Name}";
}
