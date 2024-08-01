namespace ODK.Core.Members;
public class MemberChapterPrivacySettings
{
    public Guid ChapterId { get; set; }

    public Guid MemberId { get; set; }

    /// <summary>
    /// Whether or not a member is visible on a chapter to other members.
    /// Can only be set by super admins.
    /// </summary>
    public bool HideProfile { get; set; }
}
