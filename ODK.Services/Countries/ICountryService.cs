using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Countries;

namespace ODK.Services.Countries
{
    public interface ICountryService
    {
        Task<IReadOnlyCollection<Country>> GetCountries();

        Task<Country> GetCountry(Guid countryId);
    }
}
