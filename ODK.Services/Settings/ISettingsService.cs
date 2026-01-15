using ODK.Core.Emails;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Settings.Models;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<ServiceResult> ActivatePaymentSettings(Guid currentMemberId, Guid id);

    Task<ServiceResult> CreatePaymentSettings(
        Guid currentMemberId,
        PaymentProviderType provider,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);

    Task<SiteEmailSettings> GetSiteEmailSettings(PlatformType platform);

    Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(Guid currentMemberId);

    Task<SitePaymentSettings> GetSitePaymentSettings(Guid currentMemberId, Guid id);

    Task<ServiceResult> UpdateEmailSettings(MemberServiceRequest request, UpdateEmailSettings model);

    Task<ServiceResult> UpdatePaymentSettings(
        Guid currentMemberId,
        Guid id,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled);
}