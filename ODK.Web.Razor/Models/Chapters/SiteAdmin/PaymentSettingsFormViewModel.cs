using System.ComponentModel;
using ODK.Data.Core.Countries;

namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class PaymentSettingsFormViewModel
{
    public IReadOnlyCollection<CurrencyDto> CurrencyOptions { get; set; } = [];

    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }
}