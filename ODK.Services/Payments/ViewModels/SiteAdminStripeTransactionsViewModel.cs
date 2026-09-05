using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// What the browsing platform's Stripe account holds, and what our records make of it. Scoped to the
/// platform because its records are: each platform has its own Stripe account, and the other platform's
/// overview is on the other platform's site.
/// </summary>
public class SiteAdminStripeTransactionsViewModel
{
    public required StripePaymentAccount Account { get; init; }

    /// <summary>
    /// Why the account could not be read, where it could not be - a revoked key, a rejected request, a
    /// network failure. Set means nothing was compared, so nothing below is reported either way.
    /// </summary>
    public required string? Error { get; init; }

    public required IReadOnlyCollection<SiteAdminStripeSubscriptionViewModel> Subscriptions { get; init; }

    public required TimeZoneInfo TimeZone { get; init; }

    public required IReadOnlyCollection<SiteAdminStripeTransactionViewModel> Transactions { get; init; }

    public required IReadOnlyCollection<SiteAdminStripeUnaccountedPaymentViewModel> UnaccountedPayments { get; init; }

    public required IReadOnlyCollection<SiteAdminStripeUnaccountedSubscriptionViewModel> UnaccountedSubscriptions { get; init; }

    public bool HasFindings
        => Error != null
            || Subscriptions.Any(x => x.HasFindings)
            || Transactions.Any(x => x.HasFindings)
            || UnaccountedPayments.Count > 0
            || UnaccountedSubscriptions.Count > 0;
}
