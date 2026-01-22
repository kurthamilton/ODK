using System.ComponentModel;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class PaymentSettingsFormViewModel
{
    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }
}