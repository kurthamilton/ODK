namespace ODK.Services.Payments;

/// <summary>
/// The provider's charge a payment arrived on, and what has been given back off it.
/// </summary>
public class ExternalCharge
{
    public required decimal Amount { get; init; }

    /// <summary>
    /// The commission the provider collected for itself out of the charge, where it split one. Zero where
    /// it did not.
    /// </summary>
    public required decimal Commission { get; init; }

    public required string ExternalId { get; init; }

    public required IReadOnlyCollection<ExternalRefund> Refunds { get; init; }
}
