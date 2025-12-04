using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Chapters.ViewModels;

public class GroupSubscriptionPageViewModel : GroupPageViewModel
{
    public required ChapterPaymentSettings? ChapterPaymentSettings { get; init; }

    public required SitePaymentSettings SitePaymentSettings { get; init; }
}
