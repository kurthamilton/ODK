using ODK.Core.Chapters;
using ODK.Data.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPaymentSettingsAdminPageViewModel
{
    public required IReadOnlyCollection<CurrencyDto> Currencies { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }
}