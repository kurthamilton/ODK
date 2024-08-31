using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsPageViewModel
{
    public required IReadOnlyCollection<ChapterSubscription> ChapterSubscriptions { get; init; }

    public required ChapterMembershipSettings MembershipSettings { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }
}
