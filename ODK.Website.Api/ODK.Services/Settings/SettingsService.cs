using System.Threading.Tasks;
using ODK.Core.Settings;
using ODK.Services.Caching;

namespace ODK.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly ICacheService _cacheService;
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository, ICacheService cacheService)
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
    }
}
