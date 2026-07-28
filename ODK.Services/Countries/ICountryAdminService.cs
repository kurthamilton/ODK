using ODK.Core.Countries;
using ODK.Services.Countries.Models;

namespace ODK.Services.Countries;

public interface ICountryAdminService
{
    Task<IReadOnlyCollection<Country>> GetCountries(IMemberServiceRequest request);

    Task<Country> GetCountry(IMemberServiceRequest request, Guid countryId);

    /// <summary>The culture names available for a country - the ISO-derived set, plus any stored value.</summary>
    IReadOnlyCollection<string> GetSupportedLocales(Country country);

    /// <summary>The country's stored default locale, or the one derived from its ISO code when none is stored.</summary>
    string? ResolveDefaultLocale(Country country);

    Task<ServiceResult> UpdateCountry(IMemberServiceRequest request, Guid countryId, CountryUpdateModel model);
}
