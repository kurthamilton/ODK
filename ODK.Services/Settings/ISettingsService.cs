using ODK.Core.Emails;
using ODK.Core.Payments;
using ODK.Services.Settings.Models;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<ServiceResult> ActivatePaymentSettings(IMemberServiceRequest request, Guid id);

    Task<ServiceResult> CreatePaymentSettings(
        IMemberServiceRequest request,
        PaymentProviderType provider,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);

    Task<SiteEmailSettings> GetSiteEmailSettings(IMemberServiceRequest request);

    Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(IMemberServiceRequest request);

    Task<SitePaymentSettings> GetSitePaymentSettings(IMemberServiceRequest request, Guid id);

    Task<ServiceResult> UpdateEmailSettings(IMemberServiceRequest request, EmailSettingsUpdateModel model);

    Task<ServiceResult> UpdatePaymentSettings(
        IMemberServiceRequest request,
        Guid id,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);
}