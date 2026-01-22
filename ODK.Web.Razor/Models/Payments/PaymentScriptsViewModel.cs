using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentScriptsViewModel
{
    public required IReadOnlyCollection<PaymentProviderType> Providers { get; init; }
}