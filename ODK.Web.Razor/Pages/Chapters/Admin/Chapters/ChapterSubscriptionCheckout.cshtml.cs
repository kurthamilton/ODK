namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutModel : AdminPageModel
{
    public ChapterSubscriptionCheckoutModel()
    {
    }

    public Guid PriceId { get; private set; }

    public void OnGet(Guid id)
    {
        PriceId = id;
    }
}