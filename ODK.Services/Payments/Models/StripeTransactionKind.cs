namespace ODK.Services.Payments.Models;

/// <summary>
/// What a transaction was: the three ways money reaches us, which differ in where the metadata that matches
/// them to our records lives.
/// </summary>
/// <remarks>
/// Read from the invoice that billed the money - one billed by no invoice was billed by no subscription.
/// Never leaves the process, so the values are implicit.
/// </remarks>
public enum StripeTransactionKind
{
    None = 0,

    /// <summary>
    /// A single payment, its metadata written by checkout onto the payment intent.
    /// </summary>
    OneOff,

    /// <summary>
    /// The invoice that created a subscription. Its metadata came from checkout, so it is the one a
    /// subscription is most likely to have right.
    /// </summary>
    SubscriptionInitial,

    /// <summary>
    /// Any later invoice on a subscription. Its metadata is whatever the subscription carried when Stripe
    /// issued the invoice, so this is the kind that silently stops matching.
    /// </summary>
    SubscriptionRenewal
}
