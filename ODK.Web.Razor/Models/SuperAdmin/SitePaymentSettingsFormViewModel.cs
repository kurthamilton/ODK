using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SitePaymentSettingsFormViewModel
{
    [DisplayName("Commission (%)")]
    public decimal Commission { get; set; }

    [Required]
    public bool Enabled { get; set; }

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
