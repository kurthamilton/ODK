using System.ComponentModel;

namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class PaymentSettingsFormSubmitViewModel
{
    [DisplayName("Currency")]
    public Guid? CurrencyId { get; set; }
}
