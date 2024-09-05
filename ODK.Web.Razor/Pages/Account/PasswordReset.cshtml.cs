using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages.Account;

public class PasswordResetModel : OdkPageModel
{
    private readonly IAuthenticationService _authenticationService;

    public PasswordResetModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public string Token { get; private set; } = "";

    public void OnGet(string? token = null)
    {
        Token = token ?? "";
    }

    public async Task<IActionResult> OnPostAsync(ResetPasswordFormViewModel viewModel)
    {
        var result = await _authenticationService.ResetPasswordAsync(viewModel.Token, viewModel.NewPassword);
        AddFeedback(result, "Your password has been updated.");

        if (!result.Success)
        {
            return Page();            
        }

        return Redirect(OdkRoutes.Account.Login(null));
    }
}
