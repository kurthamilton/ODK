using System.Threading.Tasks;
using ODK.Core.Settings;

namespace ODK.Services.Settings
{
    public interface ISettingsService
    {
        Task<VersionedServiceResult<SiteSettings>> GetSiteSettings(long? currentVersion);
    }
}
