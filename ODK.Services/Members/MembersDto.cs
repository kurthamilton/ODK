using ODK.Core.Members;

namespace ODK.Services.Members;
public class MembersDto
{
    public required IReadOnlyCollection<Member> Members { get; set; }

    public required IReadOnlyCollection<MemberSubscription> Subscriptions { get; set; }
}
