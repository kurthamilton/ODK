using ODK.Core.Emails;
using ODK.Core.Settings;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<ServiceResult> AddEmailProvider(Guid currentMemberId, UpdateEmailProvider model);

    Task<ServiceResult> DeleteEmailProvider(Guid currentMemberId, Guid emailProviderId);

    Task<EmailProvider> GetEmailProvider(Guid currentMemberId, Guid emailProviderId);

    Task<IReadOnlyCollection<EmailProvider>> GetEmailProviders(Guid currentMemberId);

    Task<SiteSettings> GetSiteSettings();

    Task<ServiceResult> UpdateEmailProvider(Guid currentMemberId, Guid emailProviderId, UpdateEmailProvider model);

    Task<ServiceResult> UpdateEmailSettings(Guid currentMemberId, string fromEmailAddress, string fromEmailName, string emailTitle);

    Task<ServiceResult> UpdateInstagramSettings(Guid currentMemberId, string scraperUserAgent);    
}
