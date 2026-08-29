using ODK.Core.Payments;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactorySettings
{
    public PaymentProviderType DefaultProvider { get; init; }
}
