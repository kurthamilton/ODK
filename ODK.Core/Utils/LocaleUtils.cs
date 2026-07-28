using System.Globalization;

namespace ODK.Core.Utils;

/// <summary>
/// Culture/locale helpers built on <see cref="CultureInfo"/>. A locale is stored as a culture name (e.g.
/// "en-GB"); date/time/number formats are derived from it rather than stored separately.
/// </summary>
public static class LocaleUtils
{
    // The set of known culture names, built once, for validating stored/entered locales.
    private static readonly Lazy<HashSet<string>> CultureNames = new(BuildCultureNames);

    // ISO 3166 alpha-2 country code -> the culture names for that region, in enumeration order, built
    // once. A region can map to several cultures (e.g. en-US and es-US both have region US).
    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>> LocalesByCountry =
        new(BuildLocalesByCountry);

    /// <summary>The default culture name for a country's ISO alpha-2 code, or null if .NET has none.</summary>
    public static string? GetDefaultLocale(string? isoCode2) => GetLocalesForCountry(isoCode2).FirstOrDefault();

    /// <summary>All culture names available for a country's ISO alpha-2 code (empty if .NET has none).</summary>
    public static IReadOnlyList<string> GetLocalesForCountry(string? isoCode2)
        => !string.IsNullOrWhiteSpace(isoCode2) && LocalesByCountry.Value.TryGetValue(isoCode2, out var locales)
            ? locales
            : [];

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

    /// <summary>True if the value is a culture name the runtime recognises (e.g. "en-GB").</summary>
    public static bool IsValidLocale(string? localeName)
        => !string.IsNullOrWhiteSpace(localeName) && CultureNames.Value.Contains(localeName);

    private static HashSet<string> BuildCultureNames()
        => CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildLocalesByCountry()
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);

                if (!result.TryGetValue(region.TwoLetterISORegionName, out var locales))
                {
                    locales = new List<string>();
                    result[region.TwoLetterISORegionName] = locales;
                }

                locales.Add(culture.Name);
            }
            catch (ArgumentException)
            {
                // Some cultures don't map to a region (neutral/custom) - skip them.
            }
        }

        return result.ToDictionary(
            x => x.Key,
            x => (IReadOnlyList<string>)x.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}
