using ODK.Core.Members;

namespace ODK.Services.Payments.Models;

/// <summary>
/// One Stripe subscription, the record it belongs to, and what its metadata would have to say for the next
/// renewal to be recorded.
/// </summary>
public class StripeSubscriptionAudit
{
    /// <summary>
    /// The metadata the subscription should carry, worked out from the record it matched. Null where nothing
    /// matched, because then there is nothing to work it out from and a guess would be worse than a gap.
    /// <para>
    /// Names no payment and no checkout session on purpose. Those belong to one purchase, and a
    /// subscription's metadata is read by every renewal - see
    /// <see cref="StripeTransactionFindingType.CheckoutIdsOnSubscription"/>.
    /// </para>
    /// </summary>
    public required IReadOnlyDictionary<string, string>? ExpectedMetadata { get; init; }

    public required IReadOnlyCollection<StripeTransactionFinding> Findings { get; init; }

    /// <summary>The site subscription this bills, where it bills one. Mutually exclusive with the other.</summary>
    public required MemberSiteSubscriptionRecord? MemberSiteSubscriptionRecord { get; init; }

    /// <summary>
    /// The group subscription this bills, where it bills one. The latest record, because a renewal appends
    /// one and they all name the same member and group.
    /// </summary>
    public required MemberSubscriptionRecord? MemberSubscriptionRecord { get; init; }

    /// <summary>What the subscription carries now, parsed the way the webhook parses it.</summary>
    public required PaymentMetadataModel Metadata { get; init; }

    public required StripeSubscription Subscription { get; init; }

    public bool HasFindings => Findings.Count > 0;
}
