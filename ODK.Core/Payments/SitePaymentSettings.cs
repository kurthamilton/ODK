using ODK.Core.Platforms;

namespace ODK.Core.Payments;

public class SitePaymentSettings : IDatabaseEntity
{
    public bool Active { get; set; }

    public string ApiPublicKey { get; set; } = string.Empty;

    public string ApiSecretKey { get; set; } = string.Empty;

    public decimal Commission { get; set; }

    public bool Enabled { get; set; }

    /// <summary>
    /// The deployment whose payment provider account these settings hold the keys for. Null until a record
    /// has been told, which the site admin form is where it happens.
    /// </summary>
    public EnvironmentType? Environment { get; set; }

    public string? ExternalId { get; set; }

    public string? ExternalUrl { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public PlatformType Platform { get; set; }

    public PaymentProviderType Provider { get; set; }

    public bool SupportsRecurringPayments => Provider.SupportsRecurringPayments();
}