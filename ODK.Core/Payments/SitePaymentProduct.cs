using ODK.Core.Platforms;

namespace ODK.Core.Payments;

/// <summary>
/// The provider-side product every one of a platform's subscription prices sits under. One per platform
/// per payment settings account, so a provider account holds a single product rather than one per
/// subscription.
/// </summary>
public class SitePaymentProduct : IDatabaseEntity
{
    public string ExternalId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public PlatformType Platform { get; set; }

    public Guid SitePaymentSettingId { get; set; }
}
