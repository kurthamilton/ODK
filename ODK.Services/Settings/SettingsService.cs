using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Settings;
using ODK.Services.Caching;

namespace ODK.Services.Settings
{
    public class SettingsService : OdkAdminServiceBase, ISettingsService
    {
        private readonly ICacheService _cacheService;
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository, ICacheService cacheService,
            IChapterRepository chapterRepository)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _settingsRepository = settingsRepository;
        }

        public async Task<VersionedServiceResult<SiteSettings>> GetSiteSettings(long? currentVersion)
        {
            return await _cacheService.GetOrSetVersionedItem(
                _settingsRepository.GetSiteSettings,
                "",
                currentVersion);
        }

        public async Task<SiteSettings> GetSiteSettings()
        {
            return await _settingsRepository.GetSiteSettings();
        }

        public async Task<ServiceResult> UpdateInstagramSettings(Guid chapterId, Guid currentMemberId, bool scrape,
            string scraperUserAgent)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            SiteSettings settings = await _settingsRepository.GetSiteSettings();

            SiteSettings update = new SiteSettings(settings.GoogleMapsApiKey, settings.InstagramUsername,
                settings.InstagramPassword, scraperUserAgent, scrape, 0);

            await _settingsRepository.UpdateSiteSettings(update);

            return ServiceResult.Successful();
        }
    }
}
