using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

/// <summary>
/// What our database holds that Stripe's objects could match, as the transaction audit needs to see it.
/// </summary>
/// <remarks>
/// Scoping is the caller's: the audit compares what it is given against what it is given, so a set narrowed
/// to one platform and one environment is what makes "nothing accounts for this" mean anything.
/// <para>
/// Each id set says which of the ids the Stripe metadata names actually exist, which is all a dangling
/// reference needs and is one lookup rather than a whole table. They cover the ids the webhook path resolves
/// - members, groups, group subscriptions, site subscription prices, payments and checkout sessions. Names
/// for rendering are the service's to resolve, so the audit stays free of everything but the comparison.
/// </para>
/// </remarks>
public class StripeTransactionRecords
{
    public required IReadOnlySet<Guid> ChapterIds { get; init; }

    public required IReadOnlySet<Guid> ChapterSubscriptionIds { get; init; }

    public required IReadOnlySet<Guid> MemberIds { get; init; }

    public required IReadOnlyCollection<MemberSiteSubscriptionRecord> MemberSiteSubscriptionRecords { get; init; }

    public required IReadOnlyCollection<MemberSubscriptionRecord> MemberSubscriptionRecords { get; init; }

    public required IReadOnlySet<Guid> PaymentCheckoutSessionIds { get; init; }

    public required IReadOnlyCollection<Payment> Payments { get; init; }

    public required IReadOnlySet<Guid> SiteSubscriptionPriceIds { get; init; }
}
