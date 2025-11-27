namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LoginModel : ChapterPageModel
{
    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
    }
}
