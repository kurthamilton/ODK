using System.Text;
using System.Text.RegularExpressions;

namespace ODK.Core.Utils;

public static class StringUtils
{
    private static readonly Regex AlphaNumericRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
    private static readonly Regex TokenRegex = new Regex(@"\{(.+?)\}", RegexOptions.Compiled);

    public static string AlphaNumeric(this string text)
    {
        return AlphaNumericRegex.Replace(text, "");
    }

    public static string Interpolate(this string text, IReadOnlyDictionary<string, string> values,
        Func<string, string>? process = null)
    {
        var sb = new StringBuilder(text);

        var tokens = text.Tokens();

        foreach (string token in tokens.Where(values.ContainsKey))
        {
            var value = values[token] ?? "";
            if (process != null)
            {
                value = process(value);
            }

            sb.Replace($"{{{token}}}", value);
        }

        return sb.ToString();
    }

    public static string Pluralise(int count, string single, string? plural = null)
    {
        return count == 1
            ? single
            : (!string.IsNullOrEmpty(plural) ? plural : $"{single}s");
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
