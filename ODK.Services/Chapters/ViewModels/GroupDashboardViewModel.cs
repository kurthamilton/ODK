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
    /// Whether inviting the uploaded addresses is an outstanding action. An unpublished group can upload a
    /// list but not act on it - an invite's link lands on a group nobody outside it can see - so those rows
    /// are waiting on publication rather than on an admin.
    /// </summary>
    public bool CanInviteUploaded => WaitingToBeInvited > 0 && Chapter.IsPublished();

    /// <summary>
    /// Whether sending the invites the group is holding is the outstanding action: it is published, so the
    /// link an invite carries lands somewhere, and the admin can reach the import page that raised them.
    /// True only where <see cref="HeldInvites"/> is non-zero, so the two are never read apart.
    /// </summary>
    public required bool CanSendHeldInvites { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The steps of setting the group up. Null once every step is resolved, and for an admin who can
    /// reach none of them - a finished checklist is not a checklist of ticks, it is gone.
    /// </summary>
    public required GroupChecklistViewModel? Checklist { get; init; }

    /// <summary>
    /// Whether anything is waiting on an admin. A list that is mostly zeroes teaches people to ignore it,
    /// so the section says everything is clear rather than listing empty rows. Setting the group up is
    /// deliberately absent: those steps are the checklist's, and an action reported in both places reads
    /// as two.
    /// </summary>
    public bool HasRequiredActions =>
        CanInviteUploaded ||
        CanSendHeldInvites ||
        MembersAwaitingApproval > 0 ||
        UnrepliedContactMessages > 0;

    /// <summary>
    /// How many invites the group has raised and not emailed. Reported in two places: as what publishing
    /// will make sendable, and once published as the action that sends them. Zero where neither applies.
    /// </summary>
    public required int HeldInvites { get; init; }

    public required int? MembersAwaitingApproval { get; init; }

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

    /// <summary>
    /// Whether to offer the admin a moved page: the group is published, so a page pointing at it lands
    /// somewhere, it has not set one up, nobody has dismissed the offer, and the admin can reach the page
    /// that sets it up. Deliberately not part of <see cref="HasRequiredActions"/> - a group that never
    /// moved here from anywhere has nothing outstanding, which is why the offer can be dismissed.
    /// </summary>
    public required bool PromptMovedPage { get; init; }

    /// <summary>
    /// Whether to offer the admin the wording for telling the old community, which is the action that
    /// follows setting a moved page up. Runs out with the migration window: after that the move is not
    /// news, and a panel nobody can clear is a panel everybody stops reading.
    /// </summary>
    public required bool PromptShareMovedPage { get; init; }

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
