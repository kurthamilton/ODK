namespace ODK.Core.Chapters;

/// <summary>
/// One step of a group's checklist, resolved: what the step is, and where the group has got to with it.
/// </summary>
public class ChecklistItemState
{
    public required DateTime? CompletedUtc { get; init; }

    public required bool Dismissable { get; init; }

    public required DateTime? DismissedUtc { get; init; }

    public required ChecklistItemType Type { get; init; }

    public bool IsCompleted() => CompletedUtc != null;

    public bool IsDismissed() => DismissedUtc != null;

    /// <summary>
    /// Whether the step is off the checklist, however it got there. A dismissed step that was later done
    /// anyway carries both timestamps, and either one resolves it.
    /// </summary>
    public bool IsResolved() => IsCompleted() || IsDismissed();
}
