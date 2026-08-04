using ODK.Core.Countries;
using ODK.Data.Core;

namespace ODK.Services.Countries;

public class CountryAdminService : OdkAdminServiceBase, ICountryAdminService
{
    public CountryAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<IReadOnlyCollection<Country>> GetCountries(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.CountryRepository.GetAll());
    }

    public async Task<Country> GetCountry(IMemberServiceRequest request, Guid countryId)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.CountryRepository.GetById(countryId));
    }
}
