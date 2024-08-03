using ODK.Core.Members;

namespace ODK.Services.Members;

public class MemberFilter
{
    public List<SubscriptionStatus> Statuses { get; set; } = [];

    public List<SubscriptionType> Types { get; set; } = [];
}
