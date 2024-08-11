using ODK.Core.Countries;

namespace ODK.Services.Countries;

public interface ICountryService
{    
    Task<Country> GetCountry(Guid countryId);

    Task<IReadOnlyCollection<DistanceUnit>> GetDistanceUnits();
}
