namespace ODK.Services.Payments.Models;

public class CreateReconciliationModel
{
    public required string PaymentReference { get; init; }

    public required IReadOnlyCollection<Guid> PaymentIds { get; init; }
}
