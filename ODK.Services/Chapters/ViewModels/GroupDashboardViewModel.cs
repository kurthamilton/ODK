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
    /// Whether the group is approved, unpublished and has the picture publication requires, so publishing
    /// is the outstanding action.
    /// </summary>
    public required bool CanPublish { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether anything is waiting on an admin. A list that is mostly zeroes teaches people to ignore it,
    /// so the section says everything is clear rather than listing empty rows.
    /// </summary>
    public bool HasRequiredActions =>
        CanPublish ||
        NeedsImage ||
        MembersAwaitingApproval > 0 ||
        UnrepliedContactMessages > 0;

    public required int? MembersAwaitingApproval { get; init; }

    /// <summary>
    /// Whether the group has no picture. Publication requires one, and a group without one shows a
    /// placeholder wherever it is listed, so adding one is outstanding whatever state the group is in.
    /// </summary>
    public required bool NeedsImage { get; init; }

    /// <summary>
    /// Whether the missing picture is the only thing standing between the group and being published, so
    /// the action can say what adding one unblocks.
    /// </summary>
    public required bool NeedsImageToPublish { get; init; }

    /// <summary>
    /// The members who most recently joined, newest first. Null when the admin can't reach the members
    /// page.
    /// </summary>
    public required IReadOnlyCollection<MemberChapterWithAvatarDto>? NewestMembers { get; init; }

    public required int? UnrepliedContactMessages { get; init; }

    /// <summary>
    /// The next few events, soonest first. Null when the admin can't reach the events page.
    /// </summary>
    public required IReadOnlyCollection<EventSummaryDto>? UpcomingEvents { get; init; }
}
