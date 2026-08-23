using ODK.Core.Members;

namespace ODK.Data.Core.Members;

/// <summary>
/// A member of a chapter, with the date they joined it. The date comes from the MemberChapter row rather
/// than from the member, so it says when they joined this group rather than when they signed up.
/// </summary>
public class MemberChapterWithAvatarDto
{
    public required int? AvatarVersion { get; init; }

    public required DateTime JoinedUtc { get; init; }

    public required Member Member { get; init; }
}
