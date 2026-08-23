using ODK.Core.Chapters;
using ODK.Data.Core.Events;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// The admin landing page: things in a group that are waiting on an admin, rather than a summary of the
/// group's health. Every count is null when the admin can't reach the page that would action it, so the
/// dashboard never advertises work somebody isn't permitted to do.
/// </summary>
public class GroupDashboardViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether the group is approved, unpublished and has the picture publication requires, so publishing
    /// is the outstanding action.
    /// </summary>
    public required bool CanPublish { get; init; }

    public required int? MembersAwaitingApproval { get; init; }

    /// <summary>
    /// Whether publishing is the outstanding action but the group has no picture, so adding one is what
    /// stands between it and being published.
    /// </summary>
    public required bool NeedsImageToPublish { get; init; }

    /// <summary>The next event, so an admin can see what's coming and how the responses look.</summary>
    public required EventSummaryDto? NextEvent { get; init; }

    public required int? UnrepliedContactMessages { get; init; }

    /// <summary>
    /// Whether there is anything at all to show. A dashboard that is mostly zeroes teaches people to
    /// ignore it, so the page says everything is clear rather than listing empty rows.
    /// </summary>
    public bool HasAnything =>
        CanPublish ||
        NeedsImageToPublish ||
        MembersAwaitingApproval > 0 ||
        UnrepliedContactMessages > 0 ||
        NextEvent != null;
}
