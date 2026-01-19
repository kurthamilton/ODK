using ODK.Core.Subscriptions;

namespace ODK.Services.Chapters.ViewModels;

public class SiteAdminChapterFormViewModel : SiteAdminChapterUpdateViewModel
{
    public required IReadOnlyCollection<SiteSubscription> SiteSubscriptions { get; init; }
}