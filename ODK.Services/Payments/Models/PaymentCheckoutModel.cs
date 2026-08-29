using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Payments.Models;

/// <summary>
/// One checkout with every difference between the kinds of payment already settled: which provider takes
/// it, what plan it charges, and what the payment it creates says. Starting the checkout and recording it
/// is then one piece of code whatever is being bought.
/// </summary>
public class PaymentCheckoutModel
{
    public required decimal Amount { get; init; }

    /// <summary>The group the payment belongs to, or null where it is the site being paid.</summary>
    public required Guid? ChapterId { get; init; }

    /// <summary>The group's provider account, where the money lands on one.</summary>
    public required ChapterPaymentAccount? ConnectedAccount { get; init; }

    public required Guid CurrencyId { get; init; }

    public required PaymentMetadataModel Metadata { get; init; }

    public required Guid PaymentCheckoutSessionId { get; init; }

    public required Guid PaymentId { get; init; }

    /// <summary>
    /// The platform the payment is taken on. Stated rather than read from <see cref="Metadata"/>, whose
    /// platform is nullable because the provider's own copy of it can predate the field.
    /// </summary>
    public required PlatformType Platform { get; init; }

    public required ExternalSubscriptionPlan Plan { get; init; }

    /// <summary>
    /// The provider the plan was read from, resolved rather than named: producing the plan needs it
    /// already, and resolving it a second time here would let the two disagree.
    /// </summary>
    public required IPaymentProvider Provider { get; init; }

    public required string Reference { get; init; }

    public required string ReturnPath { get; init; }
}
