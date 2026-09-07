using ODK.Core.Subscriptions;

namespace ODK.Services.Members.Models;

/// <summary>
/// How many more people a group can be sent invites for, measured against the group owner's plan.
/// Outstanding invites are counted alongside members: an import writes invites rather than memberships, so
/// counting members alone would let a whole file through and defer every refusal to the people invited.
/// </summary>
/// <remarks>
/// Built by both the import preview and the import itself, so the two cannot disagree about whether a file
/// fits.
/// </remarks>
public class MemberImportCapacity
{
    public required int MemberCount { get; init; }

    public required int OutstandingInviteCount { get; init; }

    /// <summary>The group owner's plan, or null where they have no active subscription.</summary>
    public required SiteSubscription? OwnerSubscription { get; init; }

    /// <summary>
    /// Whether the group owner has a plan at all. A group without one takes no new members by any route,
    /// which is a different thing to say than a limit having been reached.
    /// </summary>
    public bool HasOwnerSubscription => OwnerSubscription != null;

    /// <summary>The number of members the group owner's plan allows, or null when it sets no limit.</summary>
    public int? Limit => OwnerSubscription?.MemberLimit;

    /// <summary>
    /// How many more people the group can be sent invites for, or null when the plan sets no limit.
    /// </summary>
    /// <remarks>
    /// Do not collapse the two cases into a single null-coalesce: an owner with no plan and a plan with no
    /// limit both read as null from the subscription, but the first permits nobody and the second permits
    /// any number.
    /// </remarks>
    public uint? Remaining => OwnerSubscription != null
        ? OwnerSubscription.RemainingCapacity(MemberCount + OutstandingInviteCount)
        : 0;

    public bool Fits(uint importableCount) => Remaining == null || importableCount <= Remaining;
}
