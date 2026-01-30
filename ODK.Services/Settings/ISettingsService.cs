using ODK.Core.Emails;
using ODK.Core.Payments;
using ODK.Services.Settings.Models;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<ServiceResult> ActivatePaymentSettings(MemberServiceRequest request, Guid id);

    Task<ServiceResult> CreatePaymentSettings(
        MemberServiceRequest request,
        PaymentProviderType provider,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);

    Task<SiteEmailSettings> GetSiteEmailSettings(MemberServiceRequest request);

    Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(MemberServiceRequest request);

    Task<SitePaymentSettings> GetSitePaymentSettings(MemberServiceRequest request, Guid id);

    Task<ServiceResult> UpdateEmailSettings(MemberServiceRequest request, UpdateEmailSettings model);

    Task<ServiceResult> UpdatePaymentSettings(
        MemberServiceRequest request,
        Guid id,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);
}