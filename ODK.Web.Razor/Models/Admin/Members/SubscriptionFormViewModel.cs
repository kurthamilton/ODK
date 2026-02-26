using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormViewModel : SubscriptionFormSubmitViewModel
{
    public required Currency Currency { get; init; }

    public Guid? Id { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public required bool SupportsRecurringPayments { get; init; }
}