using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Core.Chapters;

public class ChapterSubscription : IDatabaseEntity, IChapterEntity
{
    public decimal Amount { get; set; }

    public Guid ChapterId { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public EnvironmentType Environment { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public int Months { get; set; }

    public string Name { get; set; } = string.Empty;

    public PaymentProviderType PaymentProvider { get; set; }

    public bool Recurring { get; set; }

    [Obsolete]
    public Guid? SitePaymentSettingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public SubscriptionType Type { get; set; }

    public bool IsVisibleToMembers() => !Disabled;

    public string ToReference() => $"Subscription: {Name}";
}