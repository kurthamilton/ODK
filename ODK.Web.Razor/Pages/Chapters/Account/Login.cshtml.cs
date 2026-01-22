using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LoginModel : OdkPageModel
{
    public string? ReturnUrl { get; private set; }

    public async Task OnGet(string? returnUrl)
    {
        if (CurrentMemberIdOrDefault != null)
        {
            var chapter = await GetChapter();
            Response.Redirect(returnUrl ?? OdkRoutes.Groups.Group(Platform, chapter));
        }

        ReturnUrl = returnUrl;
    }
}