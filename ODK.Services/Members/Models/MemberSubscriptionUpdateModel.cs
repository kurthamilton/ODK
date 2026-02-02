using ODK.Core.Members;

namespace ODK.Services.Members.Models;

public class MemberSubscriptionUpdateModel
{
    public DateTime? ExpiryDate { get; set; }

    public SubscriptionType Type { get; set; }
}
