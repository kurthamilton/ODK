using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterSubscriptionsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterSubscriptionSiteAdminViewModel> Subscriptions { get; init; }
}
