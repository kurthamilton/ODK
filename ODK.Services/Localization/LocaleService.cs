using ODK.Core.Utils;
using ODK.Data.Core;

namespace ODK.Services.Localization;

public class LocaleService : ILocaleService
{
    private const string FallbackPattern = "dd/MM/yyyy";

    private readonly LocaleServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    private string? _cachedPattern;
    private Guid? _cachedPatternMemberId;

    public LocaleService(IUnitOfWork unitOfWork, LocaleServiceSettings settings)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GetShortDatePattern(Guid? memberId)
    {
        if (_cachedPattern != null && _cachedPatternMemberId == memberId)
        {
            return _cachedPattern;
        }

        var locale = await ResolveLocale(memberId);
        var pattern = LocaleUtils.GetShortDatePattern(locale)
            ?? LocaleUtils.GetShortDatePattern(_settings.DefaultLocale)
            ?? FallbackPattern;

        _cachedPattern = pattern;
        _cachedPatternMemberId = memberId;
        return pattern;
    }

    private async Task<string?> ResolveLocale(Guid? memberId)
    {
        if (memberId == null)
        {
            return _settings.DefaultLocale;
        }

        var (preferences, country) = await _unitOfWork.RunAsync(
            x => x.MemberPreferencesRepository.GetByMemberId(memberId.Value),
            x => x.CountryRepository.GetByMemberIdOrDefault(memberId.Value));

        return preferences?.Locale
            ?? country?.DefaultLocale
            ?? (country != null ? LocaleUtils.GetDefaultLocale(country.IsoCode2) : null)
            ?? _settings.DefaultLocale;
    }
}
