using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services.Payments;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsPageViewModel
{
    public required ChapterPaymentSettings ChapterPaymentSettings { get; init; }

    public required IReadOnlyCollection<ChapterSubscriptionDto> ChapterSubscriptions { get; init; }

    public required Currency? Currency { get; init; }

    public required ChapterSubscription? CurrentSubscription { get; init; }

    public required ExternalSubscription? ExternalSubscription { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }    
}
