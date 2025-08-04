using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChapterViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<SiteSubscription> SiteSubscriptions { get; init; }

    public required MemberSiteSubscription? Subscription { get; init; }
}
