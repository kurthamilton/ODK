using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionSiteAdminListItemPriceViewModel
{
    public required decimal Amount { get; init; }

    public required Currency Currency { get; init; }

    public required SiteSubscriptionFrequency Frequency { get; init; }
}