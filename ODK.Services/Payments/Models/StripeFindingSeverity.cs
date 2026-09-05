namespace ODK.Services.Payments.Models;

/// <summary>
/// What something found against a Stripe account costs. Shared by the webhook audit, where it weighs an
/// unmet <see cref="StripeWebhookCheck"/>, and the transaction audit, where it weighs a
/// <see cref="StripeTransactionFinding"/>. Read only where there is something to weigh - a check that passed
/// or could not be made has nothing.
/// </summary>
public enum StripeFindingSeverity
{
    None = 0,

    /// <summary>Worth knowing, and working as it is.</summary>
    Info,

    /// <summary>
    /// Working as things stand, and resting on something neither side states - a Drunken Knitwits URL with
    /// no <c>p</c>, a cancelled subscription whose metadata would not have matched. Nothing is being lost
    /// now; the next change to either side loses it.
    /// </summary>
    Warning,

    /// <summary>Money or events are being lost, or will be.</summary>
    Error
}
