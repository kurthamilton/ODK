using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Services.Chapters.ViewModels;

public class SiteAdminChapterFormViewModel : SiteAdminChapterUpdateViewModel
{
    public required IReadOnlyDictionary<Guid, SitePaymentSettings> SitePaymentSettings { get; init; }

    public required IReadOnlyCollection<SiteSubscription> SiteSubscriptions { get; init; }
}