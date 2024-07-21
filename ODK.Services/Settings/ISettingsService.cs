using ODK.Core.Settings;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<SiteSettings> GetSiteSettings();

    Task<ServiceResult> UpdateInstagramSettings(Guid currentMemberId, string scraperUserAgent);
}
