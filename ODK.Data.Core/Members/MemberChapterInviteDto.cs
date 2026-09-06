using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberChapterInviteDto
{
    /// <summary>When the invite was raised, which is when the import that created it ran.</summary>
    public required DateTime CreatedUtc { get; init; }

    public required Member Member { get; init; }
}
