using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SitePaymentSettingsFormSubmitViewModel
{
    [DisplayName("Commission (%)")]
    public decimal Commission { get; set; }

    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// Which deployment's payment provider account these settings hold the keys for. Optional, because a
    /// row created before the field existed has none until somebody says.
    /// </summary>
    public EnvironmentType? Environment { get; set; }

    [DisplayName("External Id")]
    public string? ExternalId { get; set; }

    [DisplayName("External Url")]
    public string? ExternalUrl { get; set; }

    [Required]
    public string? Name { get; set; }

    public PaymentProviderType? Provider { get; set; }

    [Required]
    [DisplayName("Public key")]
    public string? PublicKey { get; set; }

    [Required]
    [DisplayName("Secret key")]
    public string? SecretKey { get; set; }
}
