namespace ODK.Services.Payments.Models;

/// <summary>
/// How one comparison against a Stripe webhook endpoint came out.
/// </summary>
/// <remarks>
/// Three states rather than two, because an expectation config does not state cannot be compared, and
/// reporting that as either a pass or a failure is a lie. A page showing a fabricated failure is worse than
/// no page; one showing a pass it never made is worse still.
/// </remarks>
public enum StripeWebhookCheckState
{
    None = 0,
    Met,
    Unmet,
    NotComparable
}
