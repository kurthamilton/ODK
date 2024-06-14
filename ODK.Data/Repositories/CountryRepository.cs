using ODK.Core.Countries;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

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

    public async Task<Country> GetCountry(Guid id)
    {
        return await Context
            .Select<Country>()
            .Where(x => x.Id).EqualTo(id)
            .FirstOrDefaultAsync();
    }
}
