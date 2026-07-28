using System.Globalization;

namespace ODK.Core.Utils;

/// <summary>
/// Culture/locale helpers built on <see cref="CultureInfo"/>. A locale is stored as a culture name (e.g.
/// "en-GB"); date/time/number formats are derived from it rather than stored separately.
/// </summary>
public static class LocaleUtils
{
    // ISO 3166 alpha-2 country code -> a representative culture name, built once. A region can map to
    // several cultures (e.g. en-US and es-US both have region US); the first specific culture wins.
    private static readonly Lazy<IReadOnlyDictionary<string, string>> LocalesByCountry = new(BuildLocalesByCountry);

    /// <summary>The default culture name for a country's ISO alpha-2 code, or null if .NET has none.</summary>
    public static string? GetDefaultLocale(string? isoCode2)
        => !string.IsNullOrWhiteSpace(isoCode2)
            && LocalesByCountry.Value.TryGetValue(isoCode2, out var locale)
                ? locale
                : null;

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

    private static IReadOnlyDictionary<string, string> BuildLocalesByCountry()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);

                // The first specific culture for a region wins - don't overwrite.
                if (!result.ContainsKey(region.TwoLetterISORegionName))
                {
                    result.Add(region.TwoLetterISORegionName, culture.Name);
                }
            }
            catch (ArgumentException)
            {
                // Some cultures don't map to a region (neutral/custom) - skip them.
            }
        }

        return result;
    }
}
