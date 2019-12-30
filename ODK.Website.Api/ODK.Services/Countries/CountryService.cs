using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Countries;
using ODK.Services.Caching;

namespace ODK.Services.Countries
{
    public class CountryService : ICountryService
    {
        private readonly ICacheService _cacheService;
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _countryRepository = countryRepository;
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Country>>> GetCountries(long? currentVersion)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                _countryRepository.GetCountries,
                _countryRepository.GetCountriesVersion,
                currentVersion);
        }
    }
}
