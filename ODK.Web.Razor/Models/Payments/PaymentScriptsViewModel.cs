using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentScriptsViewModel
{
    public required string ApiPublicKey { get; init; }

    public required string CurrencyCode { get; init; }

    public required PaymentProviderType Provider { get; init; }

    public required bool IsSubscription { get; init; }
}
