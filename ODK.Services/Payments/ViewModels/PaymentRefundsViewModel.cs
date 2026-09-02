namespace ODK.Services.Payments.ViewModels;

public class PaymentRefundsViewModel
{
    public required IReadOnlyCollection<PaymentRefundItemViewModel> Refunds { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
