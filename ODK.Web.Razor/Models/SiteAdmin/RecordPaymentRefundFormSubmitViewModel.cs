namespace ODK.Web.Razor.Models.SiteAdmin;

/// <summary>
/// A refund already made through the payment provider, being written down.
/// </summary>
public class RecordPaymentRefundFormSubmitViewModel
{
    public decimal Amount { get; init; }

    public string? ExternalId { get; init; }

    public string? ExternalReversalId { get; init; }

    public decimal FeeReturnedAmount { get; init; }

    public string PaymentReference { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public decimal ReversedAmount { get; init; }
}
