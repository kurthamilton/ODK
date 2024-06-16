namespace ODK.Core.Countries;

public interface ICountryRepository
{
    Task<IReadOnlyCollection<Country>> GetCountriesAsync();

    Task<long> GetCountriesVersionAsync();

    Task<Country?> GetCountryAsync(Guid id);
}
