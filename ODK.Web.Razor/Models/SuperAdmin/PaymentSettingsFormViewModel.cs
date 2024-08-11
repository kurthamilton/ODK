using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class PaymentSettingsFormViewModel
{
    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [Required]
    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }

    public string? Provider { get; set; }

    [Required]
    [DisplayName("Public key")]
    public string? PublicKey { get; set;}

    [Required]
    [DisplayName("Secret key")]
    public string? SecretKey { get; set; }
}
