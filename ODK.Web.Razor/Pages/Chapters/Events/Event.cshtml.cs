namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventModel : OdkPageModel
{
    public string Shortcode { get; private set; } = string.Empty;

    public void OnGet(string shortcode)
    {
        Shortcode = shortcode;
    }
}