namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventCheckoutModel : ChapterPageModel2
{
    public Guid EventId { get; private set; }

    public void OnGet(Guid id)
    {
        EventId = id;
    }
}
