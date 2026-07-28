using ODK.Core.Countries;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Countries.Models;

namespace ODK.Services.Countries;

public class CountryAdminService : OdkAdminServiceBase, ICountryAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public CountryAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    public IReadOnlyCollection<string> GetSupportedLocales(Country country)
    {
        var locales = LocaleUtils.GetLocalesForCountry(country.IsoCode2).ToList();

        // Keep a stored value selectable even if it isn't one of the ISO-derived cultures.
        if (!string.IsNullOrEmpty(country.DefaultLocale) &&
            !locales.Contains(country.DefaultLocale, StringComparer.OrdinalIgnoreCase))
        {
            locales.Insert(0, country.DefaultLocale);
        }

        return locales;
    }

    public string? ResolveDefaultLocale(Country country)
        => country.DefaultLocale ?? LocaleUtils.GetDefaultLocale(country.IsoCode2);

    public async Task<ServiceResult> UpdateCountry(IMemberServiceRequest request, Guid countryId, CountryUpdateModel model)
    {
        var country = await GetSiteAdminRestrictedContent(request,
            x => x.CountryRepository.GetById(countryId));

        var locale = string.IsNullOrWhiteSpace(model.DefaultLocale) ? null : model.DefaultLocale.Trim();
        if (locale != null && !LocaleUtils.IsValidLocale(locale))
        {
            return ServiceResult.Failure($"'{locale}' is not a valid locale.");
        }

        country.DefaultLocale = locale;
        _unitOfWork.CountryRepository.Update(country);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
