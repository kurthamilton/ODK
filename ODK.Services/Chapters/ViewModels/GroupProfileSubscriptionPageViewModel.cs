using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Chapters.ViewModels;

public class GroupProfileSubscriptionPageViewModel : GroupPageViewModelBase
{
    public required ChapterPaymentSettings? ChapterPaymentSettings { get; init; }

    public required SitePaymentSettings SitePaymentSettings { get; init; }
}
