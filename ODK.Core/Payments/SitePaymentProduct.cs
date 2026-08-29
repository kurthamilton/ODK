using ODK.Core.Platforms;

namespace ODK.Core.Payments;

/// <summary>
/// The provider-side product every one of a platform's subscription prices sits under. One per platform
/// per environment and provider, so a provider account holds a single product rather than one per
/// subscription.
/// </summary>
public class SitePaymentProduct : IDatabaseEntity
{
    public EnvironmentType Environment { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public PaymentProviderType PaymentProvider { get; set; }

    public PlatformType Platform { get; set; }

    [Obsolete]
    public Guid? SitePaymentSettingId { get; set; }
}
