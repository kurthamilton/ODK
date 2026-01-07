using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Chapters.SuperAdmin;

public class PaymentSettingsFormViewModel
{
    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [Required]
    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }

    [Required]
    public string? Name { get; set; }

    public PaymentProviderType? Provider { get; set; }

    [DisplayName("Public key")]
    public string? PublicKey { get; set; }

    [DisplayName("Secret key")]
    public string? SecretKey { get; set; }

    [DisplayName("Use site payment provider")]
    public bool UseSitePaymentProvider { get; set; }
}
