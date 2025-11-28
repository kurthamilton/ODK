namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutConfirmModel : ChapterPageModel
{
    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public void OnGet(Guid id, string sessionId)
    {
        SessionId = sessionId;
        SubscriptionId = SubscriptionId;
    }
}
