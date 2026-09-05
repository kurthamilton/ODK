namespace ODK.Services.Payments.Models;

/// <summary>
/// Who a renewal Stripe took and nothing here recorded was for, or why that cannot be worked out.
/// </summary>
/// <remarks>
/// Pure, so what the overview offers and what the action does are one rule rather than two: the button
/// appears against exactly the transactions this resolves, and pressing it resolves the same transaction
/// again against a fresh read of the account.
/// <para>
/// A subscription record is preferred over the metadata wherever both name something, for the reason
/// <see cref="StripeTransactionFindingType.DisagreesWithRecord"/> gives - the metadata is what a webhook
/// acts on, so where the two differ it is the metadata that is wrong.
/// </para>
/// </remarks>
public class StripeRenewalBackfill
{
    private StripeRenewalBackfill(
        Guid? memberId,
        Guid? chapterId,
        Guid? chapterSubscriptionId,
        Guid? siteSubscriptionPriceId,
        string? reason)
    {
        ChapterId = chapterId;
        ChapterSubscriptionId = chapterSubscriptionId;
        MemberId = memberId;
        Reason = reason;
        SiteSubscriptionPriceId = siteSubscriptionPriceId;
    }

    /// <summary>The group the renewal belongs to. Null on a site subscription, which belongs to none.</summary>
    public Guid? ChapterId { get; }

    public Guid? ChapterSubscriptionId { get; }

    public Guid? MemberId { get; }

    /// <summary>
    /// Why the renewal cannot be written down, said in the words a site admin is shown. Null where it can.
    /// </summary>
    public string? Reason { get; }

    public Guid? SiteSubscriptionPriceId { get; }

    public bool CanBackfill => Reason == null;

    public static StripeRenewalBackfill Resolve(
        StripeTransactionAudit audit, StripeTransactionRecords records)
    {
        var transaction = audit.Transaction;

        if (transaction.Kind != StripeTransactionKind.SubscriptionRenewal)
        {
            return Unresolved("Only a subscription renewal is written down here");
        }

        if (transaction.Status != StripeTransactionStatus.Succeeded)
        {
            return Unresolved("The renewal took no money");
        }

        if (audit.Payment != null)
        {
            return Unresolved("The renewal is already recorded");
        }

        // What the payment is recorded against, and what names the record the renewal extended. A renewal
        // carrying neither is one nothing can be written down from.
        if (string.IsNullOrEmpty(transaction.SubscriptionId))
        {
            return Unresolved("The renewal names no subscription");
        }

        var memberSubscriptionRecord = records.MemberSubscriptionRecords
            .Where(x => string.Equals(x.ExternalId, transaction.SubscriptionId, StringComparison.Ordinal))
            .OrderByDescending(x => x.PurchasedUtc)
            .FirstOrDefault();

        if (memberSubscriptionRecord?.ChapterSubscriptionId != null)
        {
            return ForChapterSubscription(
                memberSubscriptionRecord.MemberId,
                memberSubscriptionRecord.ChapterId,
                memberSubscriptionRecord.ChapterSubscriptionId.Value);
        }

        var memberSiteSubscriptionRecord = records.MemberSiteSubscriptionRecords
            .Where(x => string.Equals(x.ExternalId, transaction.SubscriptionId, StringComparison.Ordinal))
            .OrderByDescending(x => x.CreatedUtc)
            .FirstOrDefault();

        if (memberSiteSubscriptionRecord?.SiteSubscriptionPriceId != null)
        {
            return ForSiteSubscription(
                memberSiteSubscriptionRecord.MemberId,
                memberSiteSubscriptionRecord.SiteSubscriptionPriceId.Value);
        }

        return FromMetadata(audit.Metadata, records);
    }

    private static StripeRenewalBackfill ForChapterSubscription(
        Guid memberId, Guid chapterId, Guid chapterSubscriptionId)
        => new(memberId, chapterId, chapterSubscriptionId, siteSubscriptionPriceId: null, reason: null);

    private static StripeRenewalBackfill ForSiteSubscription(Guid memberId, Guid siteSubscriptionPriceId)
        => new(memberId, chapterId: null, chapterSubscriptionId: null, siteSubscriptionPriceId, reason: null);

    /* Reached only where no record of ours names the subscription, so nothing we hold has said who the
       renewal was for and the metadata is all there is. Every id it names has to exist: this is the value a
       webhook would have acted on, and one naming a row that is gone leaves the renewal unplaceable rather
       than placed somewhere wrong. */
    private static StripeRenewalBackfill FromMetadata(
        PaymentMetadataModel metadata, StripeTransactionRecords records)
    {
        if (metadata.MemberId == null || !records.MemberIds.Contains(metadata.MemberId.Value))
        {
            return Unresolved("Nothing says which member the renewal was for");
        }

        // ChapterSubscriptionId routes a group subscription, so it decides which set of ids applies - the
        // same precedence ProcessWebhookSubscription applies to the same metadata.
        if (metadata.ChapterSubscriptionId != null)
        {
            return metadata.ChapterId != null
                && records.ChapterIds.Contains(metadata.ChapterId.Value)
                && records.ChapterSubscriptionIds.Contains(metadata.ChapterSubscriptionId.Value)
                    ? ForChapterSubscription(
                        metadata.MemberId.Value,
                        metadata.ChapterId.Value,
                        metadata.ChapterSubscriptionId.Value)
                    : Unresolved("The renewal names a group subscription that does not exist");
        }

        if (metadata.SiteSubscriptionPriceId != null)
        {
            return records.SiteSubscriptionPriceIds.Contains(metadata.SiteSubscriptionPriceId.Value)
                ? ForSiteSubscription(metadata.MemberId.Value, metadata.SiteSubscriptionPriceId.Value)
                : Unresolved("The renewal names a site subscription price that does not exist");
        }

        return Unresolved("Nothing says what the renewal was for");
    }

    private static StripeRenewalBackfill Unresolved(string reason)
        => new(
            memberId: null,
            chapterId: null,
            chapterSubscriptionId: null,
            siteSubscriptionPriceId: null,
            reason);
}
