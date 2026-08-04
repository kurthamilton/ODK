using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class CountryContentViewModel
{
    public CountryContentViewModel(Country country)
    {
        Country = country;
    }

    public Country Country { get; }
}
