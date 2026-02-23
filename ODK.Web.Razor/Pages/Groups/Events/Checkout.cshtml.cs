namespace ODK.Web.Razor.Pages.Groups.Events;

public class CheckoutModel : OdkGroupPageModel
{
    public string Shortcode { get; private set; } = string.Empty;

    public void OnGet(string shortcode)
    {
        Shortcode = shortcode;
    }
}
