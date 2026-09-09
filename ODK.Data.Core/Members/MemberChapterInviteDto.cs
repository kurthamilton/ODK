using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberChapterInviteDto
{
    public required MemberChapterInvite Invite { get; init; }

    public required Member Member { get; init; }
}
