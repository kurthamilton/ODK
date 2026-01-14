namespace ODK.Web.Razor.Pages.Groups.Subscription;

public class CheckoutModel : OdkPageModel
{
    public Guid ChapterSubscriptionId { get; private set; }

    public void OnGet(Guid chapterSubscriptionId)
    {
        ChapterSubscriptionId = chapterSubscriptionId;
    }
}