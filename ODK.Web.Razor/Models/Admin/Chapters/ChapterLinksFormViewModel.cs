using ODK.Core.Chapters;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterLinksFormViewModel : ChapterLinksFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }
}
