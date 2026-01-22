using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Core.Chapters;

public class ChapterSubscription : IDatabaseEntity, IChapterEntity
{
    public double Amount { get; set; }

    public Guid ChapterId { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public string? ExternalId { get; set; }

    public string? ExternalProductId { get; set; }

    public Guid Id { get; set; }

    public int Months { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Recurring { get; set; }

    public Guid? SitePaymentSettingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public SubscriptionType Type { get; set; }

    public bool IsVisibleToMembers(IEnumerable<SitePaymentSettings> sitePaymentSettings)
        => !Disabled && IsVisibleToAdmins(sitePaymentSettings);

    public bool IsVisibleToAdmins(IEnumerable<SitePaymentSettings> sitePaymentSettings)
        => sitePaymentSettings
            .FirstOrDefault(x => x.Id == SitePaymentSettingId)?.Enabled == true;

    public string ToReference() => $"Subscription: {Name}";
}