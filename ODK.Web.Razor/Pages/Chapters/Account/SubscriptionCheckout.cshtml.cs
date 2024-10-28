using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutModel : ChapterPageModel
{
    public SubscriptionCheckoutModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid SubscriptionId { get; private set; }

    public void OnGet(Guid id)
    {
        SubscriptionId = id;
    }
}
