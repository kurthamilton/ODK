using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments;

public class PaymentContext
{
    public required EnvironmentType Environment { get; init; }

    public required PlatformType Platform { get; init; }

    public required PaymentProviderType Provider { get; init; }
}
