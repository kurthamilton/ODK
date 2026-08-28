using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentProviderFactory
{
    IPaymentProvider GetPaymentProvider(
        SitePaymentSettings sitePaymentSettings,
        ChapterPaymentAccount? paymentAccount);

    IPaymentProvider GetSitePaymentProvider(SitePaymentSettings sitePaymentSettings);

    IPaymentProvider GetSitePaymentProvider(
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        Guid? sitePaymentSettingId);

    /// <summary>
    /// The provider for these settings, or null where none is implemented for the type they name. For a
    /// caller sweeping every configured account, where a record on a provider we cannot talk to is one to
    /// pass over rather than a fault.
    /// </summary>
    IPaymentProvider? GetSitePaymentProviderOrDefault(SitePaymentSettings sitePaymentSettings);

    /// <summary>
    /// The webhook reader for these settings, or null where the provider has no such thing. Kept here rather
    /// than on <see cref="IPaymentProvider"/> so a caller never has to branch on
    /// <see cref="PaymentProviderType"/> itself.
    /// </summary>
    IStripeWebhookProvider? GetStripeWebhookProvider(SitePaymentSettings sitePaymentSettings);
}