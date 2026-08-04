using ODK.Core.Countries;

namespace ODK.Services.Countries;

public interface ICountryAdminService
{
    Task<IReadOnlyCollection<Country>> GetCountries(IMemberServiceRequest request);

    Task<Country> GetCountry(IMemberServiceRequest request, Guid countryId);
}
