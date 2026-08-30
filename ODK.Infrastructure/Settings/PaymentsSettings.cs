using ODK.Core.Payments;

namespace ODK.Infrastructure.Settings;

public class PaymentsSettings
{
    public required PaymentProviderType Active { get; init; }
}
