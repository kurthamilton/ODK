using System;

namespace ODK.Web.Common.Routes;

/// <summary>
/// What a checkout confirm page waits on. Not page routes - the status endpoint it reads and the hub it
/// watches, kept together because the two answer for the same session and have to agree on which of them
/// is entitled to.
/// </summary>
public class PaymentRoutes
{
    /// <summary>
    /// The status of a checkout session a group is the buyer of, read by one of its admins rather than by
    /// the owner the session belongs to.
    /// </summary>
    public string ChapterCheckoutSessionStatus(Guid chapterId, string externalSessionId)
        => $"/groups/{chapterId}/payments/sessions/{externalSessionId}/status";

    /// <summary>
    /// The hub a page watching a checkout session connects to. Named for what is watched on it rather than
    /// for the transport, since a hub is not a thing a caller otherwise knows about.
    /// </summary>
    /// <remarks>
    /// The group is a route value rather than a query parameter because a route value is where the request
    /// store looks for it, which is what lets a hub method compose a chapter request the way a controller
    /// does.
    /// </remarks>
    public string CheckoutSessionHub(Guid? chapterId)
        => chapterId != null
            ? $"/hubs/payments/{chapterId}"
            : "/hubs/payments";

    /// <summary>The status of a checkout session the current member is the buyer of.</summary>
    public string CheckoutSessionStatus(Guid? chapterId, string externalSessionId)
        => chapterId != null
            ? $"/payments/sessions/{externalSessionId}/status?groupId={chapterId}"
            : $"/payments/sessions/{externalSessionId}/status";
}
