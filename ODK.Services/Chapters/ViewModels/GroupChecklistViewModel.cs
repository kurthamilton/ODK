using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// The steps of setting a group up, and how far through them it is. Present on the dashboard until every
/// step is resolved, publication included - so it outlives publication by however long the group takes to
/// put its first event on.
/// </summary>
public class GroupChecklistViewModel
{
    /// <summary>
    /// Whether publishing is available now, which is what the publish step offers rather than links to.
    /// The rule is the group's, not the checklist's: an unticked step above it says what is worth doing,
    /// it does not hold publication back.
    /// </summary>
    public required bool CanPublish { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The steps this admin can reach, in the blueprint's order. A step gated behind a securable they do
    /// not hold is absent rather than shown undone, the way every other dashboard section works.
    /// </summary>
    public required IReadOnlyCollection<ChecklistItemState> Items { get; init; }

    public int CompletedCount => Items.Count(x => x.IsResolved());
}
