using ODK.Core.Countries;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormViewModel : SubscriptionFormSubmitViewModel
{
    public required Currency Currency { get; init; }

    public Guid? Id { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }

    public required bool SupportsRecurringPayments { get; init; }
}