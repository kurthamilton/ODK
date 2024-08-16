namespace ODK.Core.Payments;

public class SitePaymentSettings : IDatabaseEntity
{
    public string? ApiPublicKey { get; set; }

    public string? ApiSecretKey { get; set; }

    public Guid Id { get; set; }

    public string Provider { get; set; } = "";
}
