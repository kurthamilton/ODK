namespace ODK.Core.Members;

public class MemberChapterInvite : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    /// <summary>
    /// When the member's details were received, which is when an import staged the address rather than when
    /// this invite was raised. The retention period runs from it, so it is carried rather than restamped:
    /// re-raising an invite under a new account keeps the instant the original one held.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    /// <summary>
    /// When the invite email was sent. Null while the group is holding it: an unpublished group raises its
    /// invites and sends nothing, and publishing it sends what it is holding.
    /// </summary>
    public DateTime? SentUtc { get; set; }

    /// <summary>
    /// Emailed to the member as part of the invite link. See the remarks on the type.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
