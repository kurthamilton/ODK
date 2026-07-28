using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class CountryContentViewModel
{
    public CountryContentViewModel(Country country, string? derivedLocale, IReadOnlyCollection<string> locales)
    {
        Country = country;
        DerivedLocale = derivedLocale;
        Locales = locales;
    }

    public Country Country { get; }

    public string? DerivedLocale { get; }

    public IReadOnlyCollection<string> Locales { get; }
}
