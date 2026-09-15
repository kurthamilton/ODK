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

    /// <summary>
    /// Whether everything the checklist puts before <paramref name="type"/> is behind the group. The
    /// order is the rule: a step above another is one the checklist means to be dealt with first, and an
    /// optional one counts as dealt with once it is skipped.
    /// </summary>
    /// <remarks>
    /// False for a step the group does not have, which is the safe answer - a step that is not on this
    /// group's checklist is not one it can be waiting to reach.
    /// </remarks>
    public bool PrecedingStepsResolved(ChecklistItemType type)
    {
        var resolved = new List<ChecklistItemState>();

        foreach (var item in Items)
        {
            if (item.Type == type)
            {
                return resolved.All(x => x.IsResolved());
            }

            resolved.Add(item);
        }

        return false;
    }
}
