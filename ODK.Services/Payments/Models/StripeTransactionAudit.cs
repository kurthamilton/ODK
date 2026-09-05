using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

/// <summary>
/// One Stripe transaction, what its metadata names, and what the audit found about it.
/// </summary>
public class StripeTransactionAudit
{
    /// <summary>Empty where nothing is wrong, and where the transaction took no money to be wrong about.</summary>
    public required IReadOnlyCollection<StripeTransactionFinding> Findings { get; init; }

    /// <summary>
    /// The metadata parsed the way the webhook parses it, so a key the app cannot read shows up here as
    /// absent rather than as present and ignored.
    /// </summary>
    public required PaymentMetadataModel Metadata { get; init; }

    /// <summary>The payment this answers, where one was found.</summary>
    public required Payment? Payment { get; init; }

    public required StripeTransaction Transaction { get; init; }

    public bool HasFindings => Findings.Count > 0;
}
