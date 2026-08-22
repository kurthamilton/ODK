using ODK.Core.Members;

namespace ODK.Data.Core.Members;

/// <summary>
/// An outstanding invitation and the member it names, which is what the admin list of invited members shows.
/// </summary>
/// <remarks>
/// A projection rather than the <see cref="MemberChapterInvite"/> entity, so the invitation's token stays in
/// the data layer: the token is what lets whoever holds it activate that account, and a page listing names and
/// dates has no business carrying one.
/// </remarks>
public class MemberChapterInviteDto
{
    /// <summary>When the invitation was raised, which is when the import that created it ran.</summary>
    public required DateTime CreatedUtc { get; init; }

    public required Member Member { get; init; }
}
