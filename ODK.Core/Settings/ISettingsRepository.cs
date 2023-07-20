using System.Threading.Tasks;

namespace ODK.Core.Settings
{
    public interface ISettingsRepository
    {
        Task<SiteSettings?> GetSiteSettings();

        Task UpdateSiteSettings(SiteSettings settings);
    }
}
