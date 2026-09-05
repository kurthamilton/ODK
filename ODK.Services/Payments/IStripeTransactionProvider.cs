using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

/// <summary>
/// Reads everything one Stripe account holds: what it took, and what it bills on a schedule.
/// </summary>
/// <remarks>
/// Deliberately outside <see cref="IPaymentProvider"/>, for the reason given on
/// <see cref="IStripeWebhookProvider"/> - a caller wanting this asks the factory for it and gets nothing back
/// where the provider does not offer it.
/// <para>
/// Both reads are whole-account sweeps with no filter. An overview that hides the oldest records hides the
/// ones most likely to be wrong.
/// </para>
/// </remarks>
public interface IStripeTransactionProvider
{
    Task<IReadOnlyCollection<StripeSubscription>> ListSubscriptions();

    Task<IReadOnlyCollection<StripeTransaction>> ListTransactions();
}
