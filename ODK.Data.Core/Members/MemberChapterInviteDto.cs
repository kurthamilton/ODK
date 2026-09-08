using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberChapterInviteDto
{
    /// <summary>When the invite was raised, which is when the import that created it ran.</summary>
    public required DateTime CreatedUtc { get; init; }

    public required Member Member { get; init; }

    /// <summary>When the invite email went out, or null while the group is still holding it.</summary>
    public required DateTime? SentUtc { get; init; }
}
