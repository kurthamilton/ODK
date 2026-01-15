using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPaymentSettingsAdminPageViewModel
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }
}