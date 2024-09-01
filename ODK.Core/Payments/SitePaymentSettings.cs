namespace ODK.Core.Payments;

public class SitePaymentSettings : IDatabaseEntity, IPaymentSettings
{
    public string ApiPublicKey { get; set; } = "";

    public string ApiSecretKey { get; set; } = "";

    public bool HasApiKey => !string.IsNullOrEmpty(ApiPublicKey) && !string.IsNullOrEmpty(ApiSecretKey);

    public Guid Id { get; set; }

    public PaymentProviderType Provider { get; set; }    

    PaymentProviderType? IPaymentSettings.Provider => Provider;
}
