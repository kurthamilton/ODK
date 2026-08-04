using ODK.Core.Utils;

namespace ODK.Services.Localization;

public class LocaleService : ILocaleService
{
    private readonly LocaleServiceSettings _settings;

    public LocaleService(
        LocaleServiceSettings settings)
    {
        _settings = settings;
    }

    public string GetShortDatePattern(IServiceRequest request)
    {
        var locale = request.HttpRequestContext.Locale;

        return LocaleUtils.GetShortDatePattern(locale)
            ?? LocaleUtils.GetShortDatePattern(_settings.DefaultLocale)
            ?? throw new InvalidOperationException(
                $"Localisation:DefaultLocale '{_settings.DefaultLocale}' is not a valid culture.");
    }
}
