using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SitePaymentSettingsFormSubmitViewModel
{
    [DisplayName("Commission (%)")]
    public decimal Commission { get; set; }

    [Required]
    public bool Enabled { get; set; }

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
