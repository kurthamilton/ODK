using ODK.Core.Countries;

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
        return await _countryRepository.GetCountries();
    }

    public async Task<Country> GetCountry(Guid countryId)
    {
        return await _countryRepository.GetCountry(countryId);
    }
}
