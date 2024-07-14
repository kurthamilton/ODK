using ODK.Core.Countries;
using ODK.Services.Exceptions;

namespace ODK.Services.Countries;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;

    public CountryService(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<IReadOnlyCollection<Country>> GetCountries()
    {
        return await _countryRepository.GetCountriesAsync();
    }

    public async Task<Country> GetCountry(Guid countryId)
    {
        var country = await _countryRepository.GetCountryAsync(countryId);
        if (country == null)
        {
            throw new OdkNotFoundException();
        }

        return country;
    }
}
