using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentSubscriptionFormViewModel
{
    public required string CurrencyCode { get; init; }

    public required string ExternalId { get; init; }

    public required PaymentProviderType? Provider { get; init; }

    public required Guid SiteSubscriptionPriceId { get; init; }
}
