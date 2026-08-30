using ODK.Core.Payments;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminServiceSettings
{
    public required PaymentProviderType PaymentProvider { get; init; }
}
