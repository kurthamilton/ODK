using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutModel : AdminPageModel
{
    public ChapterSubscriptionCheckoutModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public Guid PriceId { get; private set; }

    public void OnGet(Guid id)
    {
        PriceId = id;
    }
}
