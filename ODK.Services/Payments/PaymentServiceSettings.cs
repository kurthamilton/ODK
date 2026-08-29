using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments;

public class PaymentServiceSettings
{
    public required EnvironmentType Environment { get; init; }

    public required PaymentProviderType PaymentProvider { get; init; }
}
