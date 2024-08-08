using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteSubscriptionPriceFormViewModel
{
    public List<Currency> CurrencyOptions { get; set; } = new();

    [DisplayName("Currency")]
    [Required]
    public Guid? CurrencyId { get; set; }

    [DisplayName("Monthly")]
    [Required]
    public int? MonthlyAmount { get; set; }

    [DisplayName("Yearly")]
    [Required]
    public int? YearlyAmount { get; set; }
}
