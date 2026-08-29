using ODK.Core.Countries;

namespace ODK.Services.Payments.Models;

public class OneOffPaymentCreateOptions : PaymentCreateOptions
{
    public required decimal Amount { get; init; }

    public required Currency Currency { get; init; }

    public required PaymentMetadataModel Metadata { get; init; }

    /// <summary>
    /// The ids the caller has already committed to, because something it created names the payment - an
    /// event ticket payment does. Stated here rather than read back out of <see cref="Metadata"/>, whose
    /// ids are nullable because the provider's own copy of them may carry neither.
    /// </summary>
    public required Guid PaymentCheckoutSessionId { get; init; }

    /// <inheritdoc cref="PaymentCheckoutSessionId"/>
    public required Guid PaymentId { get; init; }

    public required string Reference { get; init; }
}
