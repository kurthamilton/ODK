using System.Globalization;

namespace ODK.Core.Utils;

/// <summary>
/// Culture/locale helpers built on <see cref="CultureInfo"/>. A locale is a culture name (e.g. "en-GB");
/// date/time/number formats are derived from it rather than stored separately.
/// </summary>
public static class LocaleUtils
{
    /// <summary>
    /// The app-level fallback formatting culture (mirrors config <c>Localisation:DefaultLocale</c>). Used to
    /// format request-independent text - emails, notifications, exports - so it never inherits the ambient
    /// request culture. Request-driven UI formatting uses <see cref="CultureInfo.CurrentCulture"/> instead,
    /// which the request-localisation middleware sets from the same <see cref="GetPreferredLocale"/> parse.
    /// </summary>
    public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-GB");

    /// <summary>
    /// The first specific culture name (e.g. "en-GB") from the ordered <paramref name="candidates"/> - such
    /// as Accept-Language values - that the runtime recognises, canonicalised; or null if none qualifies.
    /// Neutral cultures (e.g. "en") are skipped so a region-less hint falls through to the default locale.
    /// Anything returned is a culture <see cref="GetShortDatePattern"/> (and so LocaleService) accepts.
    /// </summary>
    public static string? GetPreferredLocale(IEnumerable<string?> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate) || candidate == "*")
            {
                continue;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(candidate);
                if (!culture.IsNeutralCulture && !string.IsNullOrEmpty(culture.Name))
                {
                    return culture.Name;
                }
            }
            catch (CultureNotFoundException)
            {
                // Not a recognised culture - try the next candidate.
            }
        }

        return null;
    }

    /// <summary>The short-date pattern (e.g. "dd/MM/yyyy") for a culture name, or null if it's not valid.</summary>
    public static string? GetShortDatePattern(string? localeName)
    {
        if (string.IsNullOrWhiteSpace(localeName))
        {
            return null;
        }

        try
        {
            return CultureInfo.GetCultureInfo(localeName).DateTimeFormat.ShortDatePattern;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
