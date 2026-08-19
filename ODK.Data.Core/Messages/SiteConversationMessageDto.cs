using ODK.Core.Messages;
using ODK.Core.Utils;

namespace ODK.Data.Core.Messages;

public class SiteConversationMessageDto
{
    public required string MemberFirstName { get; init; }

    public string MemberFullName => NameUtils.FullName(MemberFirstName, MemberLastName);

    public required string MemberLastName { get; init; }

    public required SiteConversationMessage Message { get; init; }
}
