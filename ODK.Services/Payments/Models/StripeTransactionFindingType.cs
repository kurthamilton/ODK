namespace ODK.Services.Payments.Models;

/// <summary>
/// What can be wrong with a Stripe object's metadata, or with the record it should match.
/// </summary>
/// <remarks>
/// Findings rather than checks: unlike a webhook endpoint, of which there are a handful, there is one of
/// these per charge, so stating every comparison that passed would bury the ones that did not. Nothing
/// emitted means nothing found - see <see cref="StripeWebhookCheckState"/> for the opposite choice and why
/// it suits the other page.
/// </remarks>
public enum StripeTransactionFindingType
{
    None = 0,

    /// <summary>The charge and the payment it matched are not for the same amount.</summary>
    AmountDisagrees,

    /// <summary>
    /// A subscription's metadata carries the payment and checkout session of the purchase that created it.
    /// Every renewal reads them, so every renewal resolves to that first payment instead of recording its
    /// own.
    /// </summary>
    CheckoutIdsOnSubscription,

    /// <summary>
    /// A matched record and the metadata name different things. The metadata is what a webhook acts on, so
    /// it is the metadata that is wrong, whatever the record says.
    /// </summary>
    DisagreesWithRecord,

    /// <summary>The object carries no metadata at all.</summary>
    MetadataAbsent,

    /// <summary>Nothing in our database accounts for the money.</summary>
    NoDatabaseRecord,

    /// <summary>
    /// A key the webhook goes on to require is absent. The event is routed and then abandoned.
    /// </summary>
    RequiredKeyMissing,

    /// <summary>
    /// Neither <c>ChapterSubscriptionId</c> nor <c>SiteSubscriptionPriceId</c> on something a subscription
    /// billed. Nothing decides what the event is for, so it is dropped before anything else is read.
    /// </summary>
    RoutingKeyMissing,

    /// <summary>The metadata names a row that does not exist.</summary>
    UnknownReference
}
