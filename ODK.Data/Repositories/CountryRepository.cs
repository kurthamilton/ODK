using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Countries;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class CountryRepository : RepositoryBase, ICountryRepository
    {
        public CountryRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyCollection<Country>> GetCountries()
        {
            return await Context
                .Select<Country>()
                .ToArrayAsync();
        }

        public async Task<long> GetCountriesVersion()
        {
            return await Context
                .Select<Country>()
                .VersionAsync();
        }
    }
}
