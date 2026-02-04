using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Account;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ImpersonateModel : SiteAdminPageModel
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

        var request = MemberServiceRequest;
        var result = await _loginHandler.Impersonate(request, viewModel.MemberId.Value);
        if (result.Success)
        {
            return Redirect("/");
        }

        return OnGet();
    }
}