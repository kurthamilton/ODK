using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages.Account;

public class ActivateModel : OdkPageModel
{
    private readonly IAuthenticationService _authenticationService;

    public ActivateModel(IAuthenticationService authenticationService) 
    {
        _authenticationService = authenticationService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string token, ActivateFormViewModel viewModel)
    {
        var result = await _authenticationService.ActivateSiteAccountAsync(token, viewModel.Password);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback("Your account has been activated. You can now login.", FeedbackType.Success);
        return Redirect($"/Account/Login");
    }
}
