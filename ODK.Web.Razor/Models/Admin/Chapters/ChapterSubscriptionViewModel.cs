using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Subscriptions;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterSubscriptionViewModel
{
    public required Chapter Chapter { get; init; }

    public required MemberSiteSubscription? Current { get; init; }

    public required SiteSubscriptionsDto SiteSubscriptions { get; init; }
}
