using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// One of a group's subscriptions, and whether the group can see it.
/// </summary>
public class ChapterSubscriptionSiteAdminViewModel
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    /// <summary>
    /// Whether a group admin can see this subscription at all. False where it is disabled or payments are
    /// switched off for the platform, which is the case this page exists to make visible - the group's own
    /// subscriptions page filters those out, so from there the subscription simply does not exist.
    /// </summary>
    public required bool VisibleToGroupAdmins { get; init; }
}
