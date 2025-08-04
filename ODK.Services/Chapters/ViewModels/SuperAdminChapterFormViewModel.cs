using ODK.Core.Subscriptions;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChapterFormViewModel : SuperAdminChapterUpdateViewModel
{
    public required IReadOnlyCollection<SiteSubscription> SiteSubscriptions { get; init; }
}
