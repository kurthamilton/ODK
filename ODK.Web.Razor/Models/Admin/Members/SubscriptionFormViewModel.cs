using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormViewModel : SubscriptionFormSubmitViewModel
{
    public required Currency Currency { get; init; }

    public Guid? Id { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; init; }

    public required bool SupportsRecurringPayments { get; init; }
}