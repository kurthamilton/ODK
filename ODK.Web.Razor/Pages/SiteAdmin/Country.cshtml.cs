using Microsoft.AspNetCore.Mvc;
using ODK.Core.Countries;
using ODK.Services.Countries;
using ODK.Services.Countries.Models;
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

    public async Task<IActionResult> OnPostAsync(Guid id, CountryFormViewModel viewModel)
    {
        var result = await _countryAdminService.UpdateCountry(MemberServiceRequest, id, new CountryUpdateModel
        {
            DefaultLocale = viewModel.DefaultLocale
        });

        AddFeedback(result, "Country updated");

        if (!result.Success)
        {
            await LoadCountry(id);
            return Page();
        }

        return RedirectToPage();
    }

    private async Task LoadCountry(Guid id)
    {
        Country = await _countryAdminService.GetCountry(MemberServiceRequest, id);
        ContentViewModel = new CountryContentViewModel(
            Country,
            _countryAdminService.ResolveDefaultLocale(Country),
            _countryAdminService.GetSupportedLocales(Country));
    }
}
