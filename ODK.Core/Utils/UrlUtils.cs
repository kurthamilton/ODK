using System.Globalization;
using System.Text;
using ODK.Core.Web;

namespace ODK.Core.Utils;

public static class UrlUtils
{
    public static string BaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        var uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Authority);
    }

    public static string NormalisePath(string path)
    {
        path = path.EnsureLeading("/");

        var parts = path.Split('/');

        return Path.HasExtension(parts.Last())
            ? path
            : path.EnsureTrailing("/");
    }

    /// <summary>
    /// Creates a URL-safe slug from an arbitrary string (ASCII lowercase a-z0-9 and hyphens).
    /// </summary>
    public static string Slugify(
        string input)
    {
        // 1) Pre-clean common typography + meaningful symbol expansions.
        //    (Spaces around replacements help later collapsing logic.)
        input = PreprocessSlug(input.Trim());

        // 2) Remove diacritics (NFD decomposition + drop NonSpacingMarks).
        var normalized = input.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue; // drops accents
            }

            // Lowercase in an invariant way for stable URLs
            var c = char.ToLowerInvariant(ch);

            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
            }
            else
            {
                // mark as separator; we'll collapse later
                sb.Append('-');
            }
        }

        // 3) Collapse consecutive '-' and trim
        return StringUtils
            .CollapseMultipleChars(sb.ToString(), '-')
            .Trim('-');
    }

    /// <summary>
    /// Slugifies <paramref name="input"/>, appending a numeric suffix until the result is not in
    /// <paramref name="taken"/>, and keeping the result within <paramref name="maxLength"/>.
    /// Returns null when the input contains nothing sluggable (e.g. a name that is entirely
    /// punctuation or non-Latin script) — callers should store no slug rather than an empty one.
    /// </summary>
    public static string? SlugifyUnique(string input, IReadOnlySet<string> taken, int maxLength)
    {
        var baseSlug = TruncateSlug(Slugify(input), maxLength);
        if (string.IsNullOrEmpty(baseSlug))
        {
            return null;
        }

        if (!taken.Contains(baseSlug))
        {
            return baseSlug;
        }

        // taken is finite, so a free suffix always exists.
        for (var version = 2; ; version++)
        {
            var suffix = $"-{version}";
            var candidate = TruncateSlug(baseSlug, maxLength - suffix.Length) + suffix;
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    public static string Url(string baseUrl, string path)
        => UrlBuilder.Base(baseUrl).Path(path).Build();

    private static string PreprocessSlug(string slug)
    {
        // Normalize common “smart punctuation” to simple forms first.
        // Apostrophes are removed later (turned into nothing) for "men’s" -> "mens".
        slug = slug
            .Replace('\u2018', '\'') // ‘
            .Replace('\u2019', '\'') // ’
            .Replace('\u201C', '"')  // “
            .Replace('\u201D', '"')  // ”
            .Replace('\u2013', '-')  // – en dash
            .Replace('\u2014', '-')  // — em dash
            .Replace('\u2212', '-')  // − minus sign
            .Replace('\u00A0', ' '); // non-breaking space

        // Remove apostrophes entirely (not as separators): men's -> mens, rock'n'roll -> rocknroll
        slug = slug.Replace("'", "");

        // Expand a few symbols into words (space-padded to avoid accidental concatenation).
        // Feel free to tweak this list to match your product tone.
        slug = slug
            .Replace("&", " and ")
            .Replace("@", " at ")
            .Replace("+", " plus ")
            .Replace("%", " percent ");

        // Some separators should behave like spaces.
        slug = slug
            .Replace("/", " ")
            .Replace("\\", " ")
            .Replace("·", " ")
            .Replace("•", " ");

        return slug;
    }

    private static string TruncateSlug(string slug, int maxLength)
        => slug.Length <= maxLength
            ? slug
            : slug[..maxLength].TrimEnd('-');
}