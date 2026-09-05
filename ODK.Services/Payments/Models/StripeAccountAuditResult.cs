using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

/// <summary>
/// What the audit found about one Stripe account, in both directions: what Stripe holds that our records do
/// not explain, and what our records hold that Stripe does not.
/// </summary>
public class StripeAccountAuditResult
{
    /// <summary>Every subscription, those with something wrong first and newest within that.</summary>
    public required IReadOnlyCollection<StripeSubscriptionAudit> Subscriptions { get; init; }

    /// <summary>Every transaction, newest first, whatever its status.</summary>
    public required IReadOnlyCollection<StripeTransactionAudit> Transactions { get; init; }

    /// <summary>
    /// Live site subscription records naming a Stripe subscription the account does not have. Either the
    /// subscription was deleted, or the record names one in another account.
    /// </summary>
    public required IReadOnlyCollection<MemberSiteSubscriptionRecord> UnaccountedMemberSiteSubscriptionRecords { get; init; }

    /// <inheritdoc cref="UnaccountedMemberSiteSubscriptionRecords"/>
    public required IReadOnlyCollection<MemberSubscriptionRecord> UnaccountedMemberSubscriptionRecords { get; init; }

    /// <summary>
    /// Payments recorded as paid through Stripe that nothing in the account answers. A payment taken through
    /// an account no longer configured looks exactly like this, which is why it is reported rather than
    /// treated as a fault.
    /// </summary>
    public required IReadOnlyCollection<Payment> UnaccountedPayments { get; init; }

    public bool HasFindings
        => Subscriptions.Any(x => x.HasFindings)
            || Transactions.Any(x => x.HasFindings)
            || UnaccountedMemberSiteSubscriptionRecords.Count > 0
            || UnaccountedMemberSubscriptionRecords.Count > 0
            || UnaccountedPayments.Count > 0;
}
