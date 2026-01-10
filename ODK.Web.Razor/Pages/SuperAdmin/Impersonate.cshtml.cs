using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Account;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class ImpersonateModel : SuperAdminPageModel
{
    private readonly ILoginHandler _loginHandler;

    public ImpersonateModel(ILoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ImpersonateFormViewModel viewModel)
    {
        if (!ModelState.IsValid || viewModel.MemberId == null)
        {
            return OnGet();
        }

        var result = await _loginHandler.Impersonate(CurrentMemberId, viewModel.MemberId.Value);
        if (result.Success)
        {
            return Redirect("/");
        }

        return OnGet();
    }
}