using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteSubscriptionPriceFormViewModel
{
    [Required]
    public int? Amount { get; set; }

    public List<Currency> CurrencyOptions { get; set; } = new();

    [DisplayName("Currency")]
    [Required]
    public Guid? CurrencyId { get; set; }

    [Required]
    public SiteSubscriptionFrequency? Frequency { get; set; }
}