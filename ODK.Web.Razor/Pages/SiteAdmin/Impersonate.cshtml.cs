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

    public string? Search { get; private set; }

    public IActionResult OnGet(string? search)
    {
        Search = search;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(ImpersonateFormViewModel viewModel, string? search)
    {
        if (!ModelState.IsValid || viewModel.MemberId == null)
        {
            return OnGet(search);
        }

        var request = MemberServiceRequest;
        var result = await _loginHandler.AddAccount(request, viewModel.MemberId.Value);
        if (result.Success)
        {
            return Redirect("/");
        }

        return OnGet(search);
    }
}
