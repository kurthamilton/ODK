namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutModel : ChapterPageModel
{
    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}
