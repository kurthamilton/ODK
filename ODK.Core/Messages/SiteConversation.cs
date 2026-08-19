namespace ODK.Core.Messages;

/// <summary>
/// A thread between a member and the site's admins. The site counterpart of
/// <see cref="Chapters.ChapterConversation"/>, kept as its own table rather than that one with an optional
/// chapter: every read of a chapter conversation assumes it belongs to a group, and one missed would put a
/// site conversation in a group's inbox.
/// </summary>
public class SiteConversation : IDatabaseEntity, IMemberEntity
{
    public DateTime? ArchivedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    /// <summary>The member the thread belongs to. Site admins are the other side of every one of them.</summary>
    public Guid MemberId { get; set; }

    public required string Subject { get; set; }
}
