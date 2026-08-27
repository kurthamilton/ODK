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
    /// The webhook reader for these settings, or null where the provider has no such thing. Kept here rather
    /// than on <see cref="IPaymentProvider"/> so a caller never has to branch on
    /// <see cref="PaymentProviderType"/> itself.
    /// </summary>
    IStripeWebhookProvider? GetStripeWebhookProvider(SitePaymentSettings sitePaymentSettings);
}