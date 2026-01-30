namespace ODK.Core.Members;

public class MemberChapter : IDatabaseEntity, IChapterEntity
{
    public bool Approved { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Whether or not a member is visible on a chapter to other members.
    /// Can only be set by site admins.
    /// </summary>
    public bool HideProfile { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
}
