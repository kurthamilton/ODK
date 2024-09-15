using ODK.Web.Common.Config.Settings;

namespace ODK.Web.Razor.Pages.Account;

public class CreateModel : OdkPageModel
{
    public CreateModel(AppSettings appSettings)
    {
        GoogleClientId = appSettings.OAuth.Google.ClientId;
    }

    public string GoogleClientId { get; }

    public void OnGet()
    {
    }
}
