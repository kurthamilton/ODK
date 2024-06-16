using ODK.Core.Countries;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class CountryRepository : RepositoryBase, ICountryRepository
{
    public CountryRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyCollection<Country>> GetCountriesAsync()
    {
        return await Context
            .Select<Country>()
            .ToArrayAsync();
    }

    public async Task<long> GetCountriesVersionAsync()
    {
        return await Context
            .Select<Country>()
            .VersionAsync();
    }

    public async Task<Country?> GetCountryAsync(Guid id)
    {
        return await Context
            .Select<Country>()
            .Where(x => x.Id).EqualTo(id)
            .FirstOrDefaultAsync();
    }
}
