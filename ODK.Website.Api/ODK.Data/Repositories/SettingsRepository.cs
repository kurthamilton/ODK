using System.Threading.Tasks;
using ODK.Core.Settings;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
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
    }
}
