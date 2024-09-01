using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterPaymentSettingsFormViewModel
{
    [DisplayName("Currency")]
    [Required]
    public Guid? CurrencyId { get; set; }

    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [Required]
    public PaymentProviderType? Provider { get; set; }
}
