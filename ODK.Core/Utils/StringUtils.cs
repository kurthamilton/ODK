using System.Text;
using System.Text.RegularExpressions;

namespace ODK.Core.Utils;

public static class StringUtils
{
    private static readonly Regex AlphaNumericRegex = new("[^a-zA-Z0-9]", RegexOptions.Compiled);
    private static readonly string HashtagRegexPattern = "#[a-zA-Z0-9]+";
    private static readonly Regex TokenRegex = new(@"\{(.+?)\}", RegexOptions.Compiled);

    public static string AlphaNumeric(this string text) => AlphaNumericRegex.Replace(text, string.Empty);

    public static string Coalesce(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;

    public static string EnsureTrailing(this string? text, string trailingText)
        => EnsureTrailing(text, trailingText, StringComparison.OrdinalIgnoreCase);

    public static string EnsureTrailing(this string? text, string trailingText, StringComparison comparisonType)
        => text?.EndsWith(trailingText, comparisonType) == true
            ? text
            : text + trailingText;

    public static string Interpolate(this string text, IReadOnlyDictionary<string, string> values,
        Func<string, string>? process = null)
    {
        var sb = new StringBuilder(text);

        var tokens = text.Tokens();

        foreach (string token in tokens.Where(values.ContainsKey))
        {
            var value = values[token] ?? string.Empty;
            if (process != null)
            {
                value = process(value);
            }

            sb.Replace($"{{{token}}}", value);
        }

        return sb.ToString();
    }

    public static bool IsHashtag(string text)
        => Regex.IsMatch(text, $"^{HashtagRegexPattern}$");

    public static IReadOnlyCollection<string> IsolateHashtags(string text)
        => Regex.Split(text, $"({HashtagRegexPattern})");

    public static string Pluralise(int count, string single, string? plural = null)
    {
        return count == 1
            ? single
            : (!string.IsNullOrEmpty(plural) ? plural : $"{single}s");
    }

    public static string RemoveLeading(string text, string leadingText)
    {
        if (string.IsNullOrEmpty(text) ||
            string.IsNullOrEmpty(leadingText) ||
            !text.StartsWith(leadingText, StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        return text.Substring(leadingText.Length);
    }

    public static string ToCsv(IReadOnlyCollection<IReadOnlyCollection<string>> data)
    {
        var csv = new StringBuilder();

        for (var rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var row = data.ElementAt(rowIndex);

            for (var i = 0; i < row.Count; i++)
            {
                var value = row.ElementAt(i);
                AppendCsvValue(csv, value);

                if (i < row.Count - 1)
                {
                    csv.Append(',');
                }
            }

            if (rowIndex < data.Count - 1)
            {
                csv.Append(Environment.NewLine);
            }
        }

        return csv.ToString();
    }

    public static IEnumerable<string> Tokens(this string text)
    {
        MatchCollection matches = TokenRegex.Matches(text);
        foreach (Match match in matches)
        {
            yield return match.Groups[1].Value;
        }
    }

    private static void AppendCsvValue(StringBuilder csv, string value)
    {
        var mustQuote =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n');
        if (!mustQuote)
        {
            csv.Append(value);
            return;
        }

        csv.Append('"');
        foreach (var @char in value)
        {
            csv.Append(@char);
            if (@char == '"')
            {
                csv.Append('"');
            }
        }
        csv.Append('"');
    }
}