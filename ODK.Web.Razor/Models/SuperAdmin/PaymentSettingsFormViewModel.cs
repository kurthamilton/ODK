using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class PaymentSettingsFormViewModel
{
    public PaymentProviderType? Provider { get; set; }

    [Required]
    [DisplayName("Public key")]
    public string? PublicKey { get; set;}

    [Required]
    [DisplayName("Secret key")]
    public string? SecretKey { get; set; }
}
