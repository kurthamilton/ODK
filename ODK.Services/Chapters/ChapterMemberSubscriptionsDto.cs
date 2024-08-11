using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters;
public class ChapterMemberSubscriptionsDto
{
    public required IReadOnlyCollection<ChapterSubscription> ChapterSubscriptions { get; set; }

    public required ChapterMembershipSettings MembershipSettings { get; set; }

    public required MemberSubscription? MemberSubscription { get; set; }    

    public required ChapterPaymentSettings PaymentSettings { get; set; }
}
