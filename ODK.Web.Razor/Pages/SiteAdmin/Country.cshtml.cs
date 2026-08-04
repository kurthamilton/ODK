using ODK.Core.Countries;
using ODK.Services.Countries;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class CountryModel : SiteAdminPageModel
{
    private readonly ICountryAdminService _countryAdminService;

    public CountryModel(ICountryAdminService countryAdminService)
    {
        _countryAdminService = countryAdminService;
    }

    public CountryContentViewModel ContentViewModel { get; private set; } = null!;

    public Country Country { get; private set; } = null!;

    public async Task OnGetAsync(Guid id)
    {
        await LoadCountry(id);
    }

    private async Task LoadCountry(Guid id)
    {
        Country = await _countryAdminService.GetCountry(MemberServiceRequest, id);
        ContentViewModel = new CountryContentViewModel(Country);
    }
}
