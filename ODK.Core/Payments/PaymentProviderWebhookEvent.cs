namespace ODK.Core.Payments;

public class PaymentProviderWebhookEvent
{
    public required string ExternalId { get; set; }

    public PaymentProviderType PaymentProviderType { get; set; }

    public DateTime ReceivedUtc { get; set; }    
}
