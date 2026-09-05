namespace ODK.Services.Payments.Models;

/// <summary>
/// One thing wrong with a Stripe object, and the two values that show it.
/// </summary>
/// <remarks>
/// Carries the values rather than a sentence about them, so a caller renders them and a test asserts on
/// them. A finding exists only where something was found - there is no state saying it passed.
/// </remarks>
public class StripeTransactionFinding
{
    public required string? Actual { get; init; }

    public required string? Expected { get; init; }

    /// <summary>
    /// The metadata key the finding is about, where it is about one. Null where the finding is about the
    /// object as a whole - no metadata at all, no record to match.
    /// </summary>
    public required string? Key { get; init; }

    public required StripeFindingSeverity Severity { get; init; }

    public required StripeTransactionFindingType Type { get; init; }
}
