namespace ODK.Core.Countries;

public interface ICountryRepository
{
    Task<IReadOnlyCollection<Country>> GetCountries();

    Task<long> GetCountriesVersion();

    Task<Country> GetCountry(Guid id);
}
