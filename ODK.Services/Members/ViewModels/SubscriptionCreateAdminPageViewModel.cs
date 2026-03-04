using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionCreateAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Currency Currency { get; init; }

    public required Member CurrentMember { get; init; }

    public required bool HasPaymentAccount { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }

    public required PlatformType Platform { get; init; }

    public required bool SupportsRecurringPayments { get; init; }
}