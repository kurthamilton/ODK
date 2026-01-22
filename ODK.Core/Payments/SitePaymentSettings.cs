namespace ODK.Core.Payments;

public class SitePaymentSettings : IDatabaseEntity
{
    public bool Active { get; set; }

    public string ApiPublicKey { get; set; } = string.Empty;

    public string ApiSecretKey { get; set; } = string.Empty;

    public decimal Commission { get; set; }

    public bool Enabled { get; set; }

    public bool HasApiKey => !string.IsNullOrEmpty(ApiPublicKey) && !string.IsNullOrEmpty(ApiSecretKey);

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public PaymentProviderType Provider { get; set; }

    public bool SupportsRecurringPayments => Provider.SupportsRecurringPayments();
}