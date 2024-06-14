using ODK.Core.Settings;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class SettingsRepository : RepositoryBase, ISettingsRepository
{
    public SettingsRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task<SiteSettings> GetSiteSettings()
    {
        return await Context
            .Select<SiteSettings>()
            .FirstOrDefaultAsync();
    }

    public async Task UpdateSiteSettings(SiteSettings settings)
    {
        await Context
            .Update<SiteSettings>()
            .Set(x => x.ScrapeInstagram, settings.ScrapeInstagram)
            .Set(x => x.InstagramScraperUserAgent, settings.InstagramScraperUserAgent)
            .ExecuteAsync();
    }
}
