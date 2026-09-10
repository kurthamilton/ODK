using ODK.Core.Chapters;
using ODK.Data.Core.Events;
using ODK.Data.Core.Members;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// The admin landing page: what is waiting on an admin, followed by a short read on the group's events
/// and members. Every section is empty or null when the admin can't reach the page that would action it,
/// so the dashboard never advertises work somebody isn't permitted to do.
/// </summary>
public class GroupDashboardViewModel
{
    /// <summary>
    /// Whether the group is approved and unpublished, so publishing is what it is waiting for - whether or
    /// not something is still blocking it. This is what shows the publish section, which owns both faces
    /// of publication: it offers the action or accounts for what is standing in the way, and
    /// <see cref="CanPublish"/> says which of the two it is.
    /// </summary>
    public bool AwaitingPublication => CanPublish || NeedsImageToPublish;

    /// <summary>
    /// Whether the group is approved, unpublished and has the picture publication requires, so publishing
    /// is the outstanding action.
    /// </summary>
    public required bool CanPublish { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether anything is waiting on an admin. A list that is mostly zeroes teaches people to ignore it,
    /// so the section says everything is clear rather than listing empty rows. Publication is deliberately
    /// absent: it is the publish section's subject, and an action reported in both places reads as two.
    /// </summary>
    public bool HasRequiredActions =>
        NeedsImageAsAction ||
        MembersAwaitingApproval > 0 ||
        UnrepliedContactMessages > 0 ||
        WaitingToBeInvited > 0;

    /// <summary>
    /// How many invites the group is holding, which publishing sends. Zero unless publishing is the
    /// outstanding action, since that is the only place it is reported.
    /// </summary>
    public required int HeldInvites { get; init; }

    public required int? MembersAwaitingApproval { get; init; }

    /// <summary>
    /// Whether the group has no picture. Publication requires one, and a group without one shows a
    /// placeholder wherever it is listed, so adding one is outstanding whatever state the group is in.
    /// </summary>
    public required bool NeedsImage { get; init; }

    /// <summary>
    /// Whether the missing picture is an action in its own right, which it is wherever publication is not
    /// waiting on it - a group not approved yet, or one already published. The picture is reported once,
    /// and where it blocks publication the publish section is where it is reported.
    /// </summary>
    public bool NeedsImageAsAction => NeedsImage && !NeedsImageToPublish;

    /// <summary>
    /// Whether the missing picture is the only thing standing between the group and being published, so
    /// the publish section can name it as the blocker.
    /// </summary>
    public required bool NeedsImageToPublish { get; init; }

    /// <summary>
    /// The members who most recently joined, newest first. Null when the admin can't reach the members
    /// page.
    /// </summary>
    public required IReadOnlyCollection<MemberChapterWithAvatarDto>? NewestMembers { get; init; }

    /// <summary>
    /// Whether to invite the admin to bring an existing group's members across: nobody else has joined,
    /// nothing has been uploaded or invited, and they can reach the import page. Deliberately not part of
    /// <see cref="HasRequiredActions"/> - a group with one member is a working group, not one with
    /// something outstanding.
    /// </summary>
    public required bool PromptMemberImport { get; init; }

    public required int? UnrepliedContactMessages { get; init; }

    /// <summary>
    /// The next few events, soonest first. Null when the admin can't reach the events page.
    /// </summary>
    public required IReadOnlyCollection<EventSummaryDto>? UpcomingEvents { get; init; }

    /// <summary>
    /// How many uploaded addresses the group is holding, invited or not - the rows it still has something
    /// to do about. Null when the admin can't reach the import page.
    /// </summary>
    public required int? WaitingToBeInvited { get; init; }
}
