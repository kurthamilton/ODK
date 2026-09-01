namespace ODK.Services.Payments.ViewModels;

public class PaymentRefundsViewModel
{
    public required IReadOnlyCollection<PaymentRefundItemViewModel> Refunds { get; init; }
}
