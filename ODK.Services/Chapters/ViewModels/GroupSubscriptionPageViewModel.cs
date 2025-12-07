using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Services.Chapters.ViewModels;

public class GroupSubscriptionPageViewModel : GroupPageViewModel
{
    public required Currency Currency { get; init; }

    public required SitePaymentSettings SitePaymentSettings { get; init; }
}
