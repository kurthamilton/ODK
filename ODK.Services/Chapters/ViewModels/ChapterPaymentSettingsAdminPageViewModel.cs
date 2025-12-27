using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPaymentSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<Currency> CurrencyOptions { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required ChapterPaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }
}
