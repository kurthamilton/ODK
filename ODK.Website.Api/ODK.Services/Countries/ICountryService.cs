using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Countries;

namespace ODK.Services.Countries
{
    public interface ICountryService
    {
        Task<VersionedServiceResult<IReadOnlyCollection<Country>>> GetCountries(long? currentVersion);
    }
}
