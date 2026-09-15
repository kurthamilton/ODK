namespace ODK.Core.Chapters;

/// <summary>
/// A group's record of one checklist step. A row exists because something happened to the step, and the
/// two timestamps say which: taken, or declined. Every step that completes gets a row, including the ones
/// derived from state held elsewhere, so a group's onboarding reads in one query rather than out of the
/// half-dozen unrelated columns the steps happen to be evidenced by.
/// </summary>
public class ChapterChecklistItem : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public ChecklistItemType ChecklistItemType { get; set; }

    /// <summary>
    /// When the step was taken. The timestamp the step itself carries where it has one - the group's
    /// creation, its approval, its publication - and otherwise when the dashboard first observed it done,
    /// which for a picture or a description is the closest thing to one that exists.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// When an organiser took the step off the checklist without doing it. Set alongside
    /// <see cref="CompletedUtc"/> rather than instead of it where a dismissed step is later done anyway:
    /// both happened, and the timeline should say so.
    /// </summary>
    public DateTime? DismissedUtc { get; set; }

    /// <summary>
    /// Whether the step is off the checklist, however it got there.
    /// </summary>
    public bool IsResolved() => CompletedUtc != null || DismissedUtc != null;
}
