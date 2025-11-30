namespace ODK.Services.Payments.Models;

public class RemotePaymentModel
{
    public required decimal Amount { get; init; }

    public required DateTime Created { get; init; }

    public required string Currency { get; init; }

    public required string? CustomerEmail { get; init; }

    public required string PaymentId { get; init; }

    public required string? SubscriptionId { get; init; }
}
