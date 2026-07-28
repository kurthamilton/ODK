using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Utils;

namespace ODK.Services.Localization;

public class LocaleService : ILocaleService
{
    private readonly LocaleServiceSettings _settings;

    public LocaleService(LocaleServiceSettings settings)
    {
        _settings = settings;
    }

    public string GetShortDatePattern(MemberPreferences? preferences, Country? country)
    {
        var locale = preferences?.Locale
            ?? country?.DefaultLocale
            ?? (country != null ? LocaleUtils.GetDefaultLocale(country.IsoCode2) : null)
            ?? _settings.DefaultLocale;

        return LocaleUtils.GetShortDatePattern(locale)
            ?? LocaleUtils.GetShortDatePattern(_settings.DefaultLocale)
            ?? throw new InvalidOperationException(
                $"Localisation:DefaultLocale '{_settings.DefaultLocale}' is not a valid culture.");
    }
}
