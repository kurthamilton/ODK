namespace ODK.Core.Chapters;

/// <summary>
/// One step of the group checklist, as the site defines it: what the step is, where it comes in the
/// sequence, and whether an organiser may decline it. The rows are the checklist - a group's own record
/// says only which of them it has taken - so reordering or extending it is a data change.
/// </summary>
public class ChecklistItem : IDatabaseEntity
{
    /// <summary>
    /// Whether an organiser may take the step off their checklist without doing it. A step that cannot be
    /// dismissed is a required one; there is no separate flag saying so.
    /// </summary>
    public required bool Dismissable { get; set; }

    public required int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// The step's name as the row states it, so a reader of the database is not left holding an integer.
    /// The label the checklist renders is copy, and lives in the view with the rest of it.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Which step this is. Unique rather than the key: the row is identified the way every other row here
    /// is, and this is what a group's record of the step points at.
    /// </summary>
    public ChecklistItemType Type { get; set; }
}
