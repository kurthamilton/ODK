using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// One of a group's subscriptions alongside the payment settings it transacts through.
/// </summary>
public class ChapterSubscriptionSiteAdminViewModel
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    /// <summary>
    /// Whether a group admin can see this subscription at all. False where the payment settings are
    /// missing or disabled, which is the case this page exists to make visible - the group's own
    /// subscriptions page filters those out, so from there the subscription simply does not exist.
    /// </summary>
    public required bool VisibleToGroupAdmins { get; init; }

    /// <summary>
    /// The settings the subscription names, or null where it names none or names one that no longer
    /// exists. Both leave it unsellable, and both look the same to the group.
    /// </summary>
    public required SitePaymentSettings? SitePaymentSettings { get; init; }
}
