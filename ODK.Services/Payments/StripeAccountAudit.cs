using System.Globalization;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

/// <summary>
/// Compares what a Stripe account holds against the records that should account for it, in both directions.
/// </summary>
/// <remarks>
/// Pure and dependency-free, so every rule is reachable from a test without a Stripe account and without a
/// database. The rules are the webhook path's own - see <see cref="PaymentService"/> - because a finding is a
/// statement about what a webhook would do with the object, not an opinion about how it ought to look.
/// </remarks>
public static class StripeAccountAudit
{
    /* A subscription bills months apart, so a payment recorded within hours of an invoice is the one that
       invoice produced. Matched on time because every payment of a subscription carries the same external
       id, and required to be the only candidate in the window: two mean the wrong one could be picked, and a
       transaction tied to the wrong payment reads as findings that are all false. */
    private static readonly TimeSpan SubscriptionPaymentMatchWindow = TimeSpan.FromHours(6);

    public static StripeAccountAuditResult Audit(
        StripePaymentAccount account,
        IReadOnlyCollection<StripeTransaction> transactions,
        IReadOnlyCollection<StripeSubscription> subscriptions,
        StripeTransactionRecords records)
    {
        var paymentsByChargeId = records.Payments
            .Where(x => !string.IsNullOrEmpty(x.ExternalChargeId))
            .GroupBy(x => x.ExternalChargeId!, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

        // A lookup rather than a dictionary: every renewal of a subscription records a payment under the
        // subscription's own id, so this side is one-to-many by design.
        var paymentsByExternalId = records.Payments
            .Where(x => !string.IsNullOrEmpty(x.ExternalId))
            .ToLookup(x => x.ExternalId!, StringComparer.Ordinal);

        var paymentsById = records.Payments.ToDictionary(x => x.Id);
        var paymentIds = paymentsById.Keys.ToHashSet();

        var matchedPaymentIds = new HashSet<Guid>();
        var transactionAudits = new List<StripeTransactionAudit>();

        foreach (var transaction in transactions.OrderByDescending(x => x.CreatedUtc))
        {
            var metadata = PaymentMetadataModel.FromDictionary(transaction.Metadata);

            var payment = MatchPayment(
                transaction, metadata, paymentsByChargeId, paymentsByExternalId, paymentsById);

            if (payment != null)
            {
                matchedPaymentIds.Add(payment.Id);
            }

            transactionAudits.Add(new StripeTransactionAudit
            {
                Findings = TransactionFindings(transaction, metadata, records, paymentIds, payment),
                Metadata = metadata,
                Payment = payment,
                Transaction = transaction
            });
        }

        var subscriptionIds = subscriptions.Select(x => x.Id).ToHashSet(StringComparer.Ordinal);

        return new StripeAccountAuditResult
        {
            Subscriptions =
            [
                .. subscriptions
                    .Select(x => AuditSubscription(account, x, records))
                    .OrderByDescending(x => x.HasFindings)
                    .ThenByDescending(x => x.Subscription.CreatedUtc)
            ],
            Transactions = [.. transactionAudits],
            UnaccountedMemberSiteSubscriptionRecords =
            [
                .. records.MemberSiteSubscriptionRecords
                    .Where(x => IsUnaccounted(x.IsCurrent, x.CancelledUtc, x.ExternalId, subscriptionIds))
                    .OrderByDescending(x => x.CreatedUtc)
            ],
            UnaccountedMemberSubscriptionRecords =
            [
                .. records.MemberSubscriptionRecords
                    .Where(x => IsUnaccounted(x.IsCurrent, x.CancelledUtc, x.ExternalId, subscriptionIds))
                    .OrderByDescending(x => x.PurchasedUtc)
            ],
            UnaccountedPayments =
            [
                .. records.Payments
                    .Where(x => x.PaidUtc != null
                        && x.PaymentProvider == PaymentProviderType.Stripe
                        && !matchedPaymentIds.Contains(x.Id))
                    .OrderByDescending(x => x.PaidUtc)
            ]
        };
    }

    /// <summary>
    /// Compared only where both sides state a value: a key the metadata does not carry is already reported
    /// as missing, and reporting it again as a disagreement says the same thing twice.
    /// </summary>
    private static void AddDisagreementFinding(
        List<StripeTransactionFinding> findings,
        Guid? metadataValue,
        Guid? recordValue,
        string key,
        StripeFindingSeverity severity)
    {
        if (metadataValue == null || recordValue == null || metadataValue == recordValue)
        {
            return;
        }

        findings.Add(Finding(
            StripeTransactionFindingType.DisagreesWithRecord,
            severity,
            key,
            actual: metadataValue.Value.ToString(),
            expected: recordValue.Value.ToString()));
    }

    /// <summary>
    /// A one-off reaches the webhook path through its checkout session, which resolves the payment and the
    /// session before anything else, then hands a purchase that is not a ticket to the group subscription
    /// path.
    /// </summary>
    private static void AddOneOffMetadataFindings(
        List<StripeTransactionFinding> findings,
        PaymentMetadataModel metadata,
        StripeTransactionRecords records,
        IReadOnlySet<Guid> paymentIds)
    {
        var severity = StripeFindingSeverity.Error;

        AddRequiredKeyFinding(findings, metadata.MemberId, PaymentMetadataModel.Keys.MemberId, severity);
        AddRequiredKeyFinding(findings, metadata.PaymentId, PaymentMetadataModel.Keys.PaymentId, severity);
        AddRequiredKeyFinding(
            findings,
            metadata.PaymentCheckoutSessionId,
            PaymentMetadataModel.Keys.PaymentCheckoutSessionId,
            severity);

        if (metadata.EventTicketPaymentId == null)
        {
            AddRequiredKeyFinding(
                findings, metadata.ChapterId, PaymentMetadataModel.Keys.ChapterId, severity);
            AddRequiredKeyFinding(
                findings,
                metadata.ChapterSubscriptionId,
                PaymentMetadataModel.Keys.ChapterSubscriptionId,
                severity);
        }

        AddUnknownReferenceFinding(
            findings, metadata.ChapterId, records.ChapterIds, PaymentMetadataModel.Keys.ChapterId, severity);
        AddUnknownReferenceFinding(
            findings,
            metadata.ChapterSubscriptionId,
            records.ChapterSubscriptionIds,
            PaymentMetadataModel.Keys.ChapterSubscriptionId,
            severity);
        AddUnknownReferenceFinding(
            findings, metadata.MemberId, records.MemberIds, PaymentMetadataModel.Keys.MemberId, severity);
        AddUnknownReferenceFinding(
            findings,
            metadata.PaymentCheckoutSessionId,
            records.PaymentCheckoutSessionIds,
            PaymentMetadataModel.Keys.PaymentCheckoutSessionId,
            severity);
        AddUnknownReferenceFinding(
            findings, metadata.PaymentId, paymentIds, PaymentMetadataModel.Keys.PaymentId, severity);
    }

    private static void AddRequiredKeyFinding(
        List<StripeTransactionFinding> findings,
        Guid? value,
        string key,
        StripeFindingSeverity severity)
    {
        if (value == null)
        {
            findings.Add(Finding(StripeTransactionFindingType.RequiredKeyMissing, severity, key));
        }
    }

    private static void AddSubscriptionDisagreementFindings(
        List<StripeTransactionFinding> findings,
        PaymentMetadataModel metadata,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        MemberSiteSubscriptionRecord? memberSiteSubscriptionRecord,
        StripeFindingSeverity severity)
    {
        if (memberSubscriptionRecord != null)
        {
            AddDisagreementFinding(
                findings,
                metadata.MemberId,
                memberSubscriptionRecord.MemberId,
                PaymentMetadataModel.Keys.MemberId,
                severity);
            AddDisagreementFinding(
                findings,
                metadata.ChapterId,
                memberSubscriptionRecord.ChapterId,
                PaymentMetadataModel.Keys.ChapterId,
                severity);
            AddDisagreementFinding(
                findings,
                metadata.ChapterSubscriptionId,
                memberSubscriptionRecord.ChapterSubscriptionId,
                PaymentMetadataModel.Keys.ChapterSubscriptionId,
                severity);
        }

        if (memberSiteSubscriptionRecord != null)
        {
            AddDisagreementFinding(
                findings,
                metadata.MemberId,
                memberSiteSubscriptionRecord.MemberId,
                PaymentMetadataModel.Keys.MemberId,
                severity);
            AddDisagreementFinding(
                findings,
                metadata.SiteSubscriptionPriceId,
                memberSiteSubscriptionRecord.SiteSubscriptionPriceId,
                PaymentMetadataModel.Keys.SiteSubscriptionPriceId,
                severity);
        }
    }

    /// <summary>
    /// The findings a subscription's metadata raises - the routing key that decides what the event is for,
    /// the keys the path it routes to goes on to require, and whether what it names exists. Shared with a
    /// transaction a subscription billed, which reaches the webhook path the same way and so has to satisfy
    /// the same rules.
    /// </summary>
    private static void AddSubscriptionMetadataFindings(
        List<StripeTransactionFinding> findings,
        PaymentMetadataModel metadata,
        StripeTransactionRecords records,
        StripeFindingSeverity severity)
    {
        if (metadata.ChapterSubscriptionId == null && metadata.SiteSubscriptionPriceId == null)
        {
            findings.Add(Finding(
                StripeTransactionFindingType.RoutingKeyMissing,
                severity,
                expected: $"{PaymentMetadataModel.Keys.ChapterSubscriptionId} or " +
                    PaymentMetadataModel.Keys.SiteSubscriptionPriceId));
            return;
        }

        AddRequiredKeyFinding(findings, metadata.MemberId, PaymentMetadataModel.Keys.MemberId, severity);

        // ChapterSubscriptionId is what routes a group subscription, so it decides which set of required
        // keys applies - even where a SiteSubscriptionPriceId sits beside it.
        if (metadata.ChapterSubscriptionId != null)
        {
            AddRequiredKeyFinding(findings, metadata.ChapterId, PaymentMetadataModel.Keys.ChapterId, severity);
        }

        AddUnknownReferenceFinding(
            findings, metadata.ChapterId, records.ChapterIds, PaymentMetadataModel.Keys.ChapterId, severity);
        AddUnknownReferenceFinding(
            findings,
            metadata.ChapterSubscriptionId,
            records.ChapterSubscriptionIds,
            PaymentMetadataModel.Keys.ChapterSubscriptionId,
            severity);
        AddUnknownReferenceFinding(
            findings, metadata.MemberId, records.MemberIds, PaymentMetadataModel.Keys.MemberId, severity);
        AddUnknownReferenceFinding(
            findings,
            metadata.SiteSubscriptionPriceId,
            records.SiteSubscriptionPriceIds,
            PaymentMetadataModel.Keys.SiteSubscriptionPriceId,
            severity);
    }

    private static void AddUnknownReferenceFinding(
        List<StripeTransactionFinding> findings,
        Guid? value,
        IReadOnlySet<Guid> existing,
        string key,
        StripeFindingSeverity severity)
    {
        if (value != null && !existing.Contains(value.Value))
        {
            findings.Add(Finding(
                StripeTransactionFindingType.UnknownReference,
                severity,
                key,
                actual: value.Value.ToString()));
        }
    }

    private static StripeSubscriptionAudit AuditSubscription(
        StripePaymentAccount account,
        StripeSubscription subscription,
        StripeTransactionRecords records)
    {
        var metadata = PaymentMetadataModel.FromDictionary(subscription.Metadata);

        var memberSubscriptionRecord = records.MemberSubscriptionRecords
            .Where(x => string.Equals(x.ExternalId, subscription.Id, StringComparison.Ordinal))
            .OrderByDescending(x => x.PurchasedUtc)
            .FirstOrDefault();

        var memberSiteSubscriptionRecord = records.MemberSiteSubscriptionRecords
            .Where(x => string.Equals(x.ExternalId, subscription.Id, StringComparison.Ordinal))
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefault();

        /* What a broken subscription costs depends on whether Stripe will bill it again: one that will is
           losing a renewal every period, one that will not never has to be right again. Both are listed - a
           cancelled subscription's metadata is often the evidence for what went wrong. */
        var severity = subscription.BillsAgain
            ? StripeFindingSeverity.Error
            : StripeFindingSeverity.Warning;

        var findings = new List<StripeTransactionFinding>();

        if (subscription.Metadata.Count == 0)
        {
            findings.Add(Finding(StripeTransactionFindingType.MetadataAbsent, severity));
        }
        else
        {
            AddSubscriptionMetadataFindings(findings, metadata, records, severity);
            AddSubscriptionDisagreementFindings(
                findings, metadata, memberSubscriptionRecord, memberSiteSubscriptionRecord, severity);
        }

        if (memberSubscriptionRecord == null && memberSiteSubscriptionRecord == null)
        {
            findings.Add(Finding(StripeTransactionFindingType.NoDatabaseRecord, severity));
        }

        return new StripeSubscriptionAudit
        {
            ExpectedMetadata = ExpectedMetadata(
                account, memberSubscriptionRecord, memberSiteSubscriptionRecord),
            Findings = findings,
            MemberSiteSubscriptionRecord = memberSiteSubscriptionRecord,
            MemberSubscriptionRecord = memberSubscriptionRecord,
            Metadata = metadata,
            Subscription = subscription
        };
    }

    private static IReadOnlyDictionary<string, string>? ExpectedMetadata(
        StripePaymentAccount account,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        MemberSiteSubscriptionRecord? memberSiteSubscriptionRecord)
    {
        if (memberSubscriptionRecord?.ChapterSubscriptionId != null)
        {
            return PaymentMetadataModel
                .ForChapterSubscription(
                    account.Platform,
                    memberSubscriptionRecord.MemberId,
                    memberSubscriptionRecord.ChapterId,
                    memberSubscriptionRecord.ChapterSubscriptionId.Value)
                .ToDictionary();
        }

        if (memberSiteSubscriptionRecord?.SiteSubscriptionPriceId != null)
        {
            return PaymentMetadataModel
                .ForSiteSubscription(
                    account.Platform,
                    memberSiteSubscriptionRecord.MemberId,
                    memberSiteSubscriptionRecord.SiteSubscriptionPriceId.Value)
                .ToDictionary();
        }

        return null;
    }

    private static StripeTransactionFinding Finding(
        StripeTransactionFindingType type,
        StripeFindingSeverity severity,
        string? key = null,
        string? actual = null,
        string? expected = null)
        => new()
        {
            Actual = actual,
            Expected = expected,
            Key = key,
            Severity = severity,
            Type = type
        };

    private static bool IsUnaccounted(
        bool isCurrent,
        DateTime? cancelledUtc,
        string? externalId,
        IReadOnlySet<string> subscriptionIds)
        => isCurrent
            && cancelledUtc == null
            && !string.IsNullOrEmpty(externalId)
            && !subscriptionIds.Contains(externalId);

    private static Payment? MatchPayment(
        StripeTransaction transaction,
        PaymentMetadataModel metadata,
        IReadOnlyDictionary<string, Payment> paymentsByChargeId,
        ILookup<string, Payment> paymentsByExternalId,
        IReadOnlyDictionary<Guid, Payment> paymentsById)
    {
        if (!string.IsNullOrEmpty(transaction.ChargeId) &&
            paymentsByChargeId.TryGetValue(transaction.ChargeId, out var byChargeId))
        {
            return byChargeId;
        }

        if (!string.IsNullOrEmpty(transaction.PaymentIntentId))
        {
            var byPaymentIntentId = paymentsByExternalId[transaction.PaymentIntentId].ToArray();
            if (byPaymentIntentId.Length == 1)
            {
                return byPaymentIntentId[0];
            }
        }

        if (!string.IsNullOrEmpty(transaction.SubscriptionId))
        {
            var bySubscriptionId = paymentsByExternalId[transaction.SubscriptionId]
                .Where(x => x.PaidUtc != null
                    && (x.PaidUtc.Value - transaction.CreatedUtc).Duration() < SubscriptionPaymentMatchWindow)
                .ToArray();
            if (bySubscriptionId.Length == 1)
            {
                return bySubscriptionId[0];
            }
        }

        /* Only a one-off's metadata is trusted to name its payment. A subscription's is read by every
           renewal, so on anything a subscription billed it names the first purchase, and matching on it
           would tie every renewal to that one payment. */
        return transaction.Kind == StripeTransactionKind.OneOff
            && metadata.PaymentId != null
            && paymentsById.TryGetValue(metadata.PaymentId.Value, out var byMetadata)
                ? byMetadata
                : null;
    }

    private static IReadOnlyCollection<StripeTransactionFinding> TransactionFindings(
        StripeTransaction transaction,
        PaymentMetadataModel metadata,
        StripeTransactionRecords records,
        IReadOnlySet<Guid> paymentIds,
        Payment? payment)
    {
        /* A transaction that took no money has nothing to reconcile against: an abandoned checkout writes no
           payment, and an open invoice has not been billed yet. Both are listed, and neither is judged. */
        if (transaction.Status != StripeTransactionStatus.Succeeded)
        {
            return [];
        }

        var findings = new List<StripeTransactionFinding>();

        if (transaction.Metadata.Count == 0)
        {
            findings.Add(Finding(StripeTransactionFindingType.MetadataAbsent, StripeFindingSeverity.Error));
        }
        else if (transaction.Kind == StripeTransactionKind.OneOff)
        {
            AddOneOffMetadataFindings(findings, metadata, records, paymentIds);
        }
        else
        {
            AddSubscriptionMetadataFindings(findings, metadata, records, StripeFindingSeverity.Error);
        }

        if (payment == null)
        {
            findings.Add(Finding(StripeTransactionFindingType.NoDatabaseRecord, StripeFindingSeverity.Error));
            return findings;
        }

        AddDisagreementFinding(
            findings,
            metadata.MemberId,
            payment.MemberId,
            PaymentMetadataModel.Keys.MemberId,
            StripeFindingSeverity.Error);

        /* What the provider says it took where that has been read back, and what we asked for otherwise. A
           warning rather than an error: a price that changed between the charge and the record, or a
           currency converted on the way, is a difference to look at rather than money that went missing. */
        var recordedAmount = payment.ActualAmount ?? payment.Amount;
        if (recordedAmount != transaction.Amount)
        {
            findings.Add(Finding(
                StripeTransactionFindingType.AmountDisagrees,
                StripeFindingSeverity.Warning,
                actual: transaction.Amount.ToString(CultureInfo.InvariantCulture),
                expected: recordedAmount.ToString(CultureInfo.InvariantCulture)));
        }

        return findings;
    }
}
