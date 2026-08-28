using ODK.Data.Core.Countries;

namespace ODK.Web.Razor.Models.Chapters.SiteAdmin;

public class PaymentSettingsFormViewModel : PaymentSettingsFormSubmitViewModel
{
    public required IReadOnlyCollection<CurrencyDto> CurrencyOptions { get; init; }
}
