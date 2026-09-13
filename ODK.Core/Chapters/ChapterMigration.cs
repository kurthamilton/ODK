namespace ODK.Core.Chapters;

/// <summary>
/// A group's move here from somewhere else, and the signpost it leaves behind on the platform it came
/// from. <see cref="MovedUtc"/> is the switch: set, the group's moved page is public and its home page
/// carries a welcome banner for as long as <see cref="ChapterMovedBannerWindow"/> allows; null, the row
/// holds the wording the organiser has written without publishing any of it.
/// </summary>
public class ChapterMigration : IChapterEntity
{
    public Guid ChapterId { get; set; }

    /// <summary>
    /// The organiser's own account of why the group moved. Optional - the moved page reads perfectly
    /// well without it, and a group that would rather not explain itself should not have to.
    /// </summary>
    public string? MessageHtml { get; set; }

    /// <summary>
    /// When the organiser declared the move. Both the switch for the moved page and the clock the
    /// home page banner runs down.
    /// </summary>
    public DateTime? MovedUtc { get; set; }

    /// <summary>
    /// When an admin said the group's dashboard should stop offering the moved page. Per group rather
    /// than per admin: whether a group came from somewhere else is a fact about the group, so one
    /// organiser answering it answers it for the others.
    /// </summary>
    public DateTime? PromptDismissedUtc { get; set; }

    /// <summary>
    /// Where the group moved from, in the organiser's words - "Meetup", "Facebook", "our mailing list".
    /// Optional, and the copy reads generically without it, because the answer is different for every
    /// group and some of them have no single name for it.
    /// </summary>
    public string? PreviousPlatformName { get; set; }

    public bool HasMoved() => MovedUtc != null;
}
