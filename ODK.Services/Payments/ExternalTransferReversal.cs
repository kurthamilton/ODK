namespace ODK.Services.Payments;

/// <summary>
/// The provider's reversal of a transfer: money taken back off a connected account.
/// </summary>
public class ExternalTransferReversal
{
    public required decimal Amount { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required string CurrencyCode { get; init; }

    public required string ExternalId { get; init; }
}
