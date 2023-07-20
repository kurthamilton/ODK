using System;
using System.Threading.Tasks;
using ODK.Core.Settings;

namespace ODK.Services.Settings
{
    public interface ISettingsService
    {
        Task<SiteSettings> GetSiteSettings();

        Task<ServiceResult> UpdateInstagramSettings(Guid chapterId, Guid currentMemberId, bool scrape, string scraperUserAgent);
    }
}
