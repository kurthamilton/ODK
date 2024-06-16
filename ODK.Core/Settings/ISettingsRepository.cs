namespace ODK.Core.Settings;

public interface ISettingsRepository
{
    Task<SiteSettings?> GetSiteSettingsAsync();

    Task UpdateSiteSettingsAsync(SiteSettings settings);
}
