namespace ODK.Web.Razor.Models.Payments;

/// <summary>
/// Which entitlement lets a page watch a checkout session, and so which status endpoint and which hub
/// method answer for it. Orthogonal to whether a group is involved: a member buying a group's membership is
/// still <see cref="Member"/>, because the session is theirs.
/// </summary>
public enum PaymentConfirmScope
{
    None,

    /// <summary>The current member is the buyer.</summary>
    Member,

    /// <summary>
    /// The group is the buyer. The session belongs to the group's owner and any admin holding the
    /// SiteSubscription securable may watch it, so the member watching is not necessarily the one who
    /// bought.
    /// </summary>
    ChapterAdmin
}
