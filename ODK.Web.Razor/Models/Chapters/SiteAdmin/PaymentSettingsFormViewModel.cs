using System.ComponentModel;
using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class PaymentSettingsFormViewModel
{
    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }

    public PaymentProviderType? Provider { get; set; }

    [DisplayName("Public key")]
    public string? PublicKey { get; set; }

    [DisplayName("Secret key")]
    public string? SecretKey { get; set; }

    [DisplayName("Use site payment provider")]
    public bool UseSitePaymentProvider { get; set; }
}