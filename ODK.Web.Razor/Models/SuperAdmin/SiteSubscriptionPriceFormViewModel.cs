using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteSubscriptionPriceFormViewModel
{
    [Required]
    public int? Amount { get; set; }

    public List<Currency> CurrencyOptions { get; set; } = new();

    [DisplayName("Currency")]
    [Required]
    public Guid? CurrencyId { get; set; }

    [DisplayName("External Id")]
    [Required]
    public string? ExternalId { get; set; }

    [Required]
    public SiteSubscriptionFrequency? Frequency { get; set; }
}
