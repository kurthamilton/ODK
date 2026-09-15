namespace ODK.Core.Chapters;

/// <summary>
/// A group's checklist as it stands, and the rows the database is missing for it. The two come out
/// together because they are one pass over the same steps: a step found complete with nothing recorded
/// against it is both a tick on the checklist and a row to write.
/// </summary>
public class ChapterChecklistResolution
{
    public required IReadOnlyCollection<ChecklistItemState> Items { get; init; }

    /// <summary>
    /// Rows for steps that have completed since the checklist was last resolved. Empty on all but the
    /// load that first observes each step, so a settled group's dashboard writes nothing.
    /// </summary>
    public required IReadOnlyCollection<ChapterChecklistItem> Unrecorded { get; init; }

    /// <summary>
    /// Whether every step is resolved, which is what takes the checklist off the dashboard for good.
    /// </summary>
    public bool IsFinished() => Items.All(x => x.IsResolved());
}
