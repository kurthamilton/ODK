using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// Everything the publication machine's state resolver and its steps read. Small, because the state is three
/// dates on the group itself - the only thing a transition needs beyond that is who to tell.
/// </summary>
public sealed class ChapterPublicationContext
{
    public required Chapter Chapter { get; init; }

    /// <summary>Whether the group has a picture, which publishing needs. Not needed to approve.</summary>
    public bool? HasImage { get; init; }

    /// <summary>
    /// How many invites the group has raised and not emailed, which publishing tells the owner about.
    /// Not needed to approve.
    /// </summary>
    public int? HeldInvites { get; init; }

    /// <summary>
    /// Whether everything the checklist puts before submission is resolved. Not needed to approve or to
    /// publish, both of which come after it.
    /// </summary>
    public bool? ChecklistReadyToSubmit { get; init; }

    /// <summary>The group's owner, who is told when it is approved and when it is published.</summary>
    public Member? Owner { get; init; }

    public required IServiceRequest Request { get; init; }

    /// <summary>
    /// The site admins told that a group is waiting on them. Not needed by any other transition.
    /// </summary>
    public IReadOnlyCollection<Member>? SiteAdmins { get; init; }

    /// <summary>The checklist, on the transition whose legality depends on it.</summary>
    public bool RequiredChecklistReadyToSubmit => ChecklistReadyToSubmit ?? throw new InvalidOperationException(
        "The transition depends on the checklist being ready to submit but it was not resolved");

    /// <summary>Whether the group has a picture, on a transition whose legality depends on it.</summary>
    public bool RequiredHasImage => HasImage ?? throw new InvalidOperationException(
        "The transition depends on the group having a picture but none was resolved");

    /// <summary>
    /// The held invites, on a transition that reports them. Nullable rather than defaulting to zero: a
    /// transition that never resolved the count would otherwise report a group holding invites as holding
    /// none, and say nothing where it had something to say.
    /// </summary>
    public int RequiredHeldInvites => HeldInvites ?? throw new InvalidOperationException(
        "The transition reports the invites the group is holding but no count was resolved");

    /// <summary>The owner, on a transition that has to tell them something.</summary>
    public Member RequiredOwner => Owner ?? throw new InvalidOperationException(
        "The transition notifies the group's owner but none was resolved");

    /// <summary>The site admins, on a transition that has to tell them something.</summary>
    public IReadOnlyCollection<Member> RequiredSiteAdmins => SiteAdmins ?? throw new InvalidOperationException(
        "The transition notifies site admins but none were resolved");
}
