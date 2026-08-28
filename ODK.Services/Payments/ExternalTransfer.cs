namespace ODK.Services.Payments;

/// <summary>
/// A movement of money out of a charge we collected and on to a connected account.
/// </summary>
public class ExternalTransfer
{
    public required decimal Amount { get; init; }

    public required string ConnectedAccountId { get; init; }

    public required string CurrencyCode { get; init; }

    /// <summary>
    /// The charge the money comes out of. Naming it lets the provider move funds that have not finished
    /// clearing, and ties the transfer to the payment it belongs to.
    /// </summary>
    public required string ExternalChargeId { get; init; }

    /// <summary>
    /// Stops a retry paying twice. Derived from our own payment, so every attempt at the same transfer
    /// carries the same key however many times the job runs.
    /// </summary>
    public required string IdempotencyKey { get; init; }
}
