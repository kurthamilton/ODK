using System.Text.RegularExpressions;

namespace ODK.Services.Emails.Validation;

/// <summary>
/// Checks a template only references placeholders the send path actually supplies. One left misspelt
/// reaches the member as literal braces, which nothing downstream can catch: interpolation leaves an
/// unrecognised token exactly as written.
/// </summary>
public static class EmailTemplateValidator
{
    private static readonly Regex PlaceholderRegex = new(EmailTemplatePattern.Value, RegexOptions.Compiled);

    /// <summary>
    /// The placeholders used in <paramref name="text"/> that are not in <paramref name="supplied"/>,
    /// in the order they first appear and without repeats.
    /// </summary>
    public static IReadOnlyCollection<string> UnknownPlaceholders(
        string? text, IReadOnlyCollection<string> supplied)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        // Matched the same way the parameters are, so a template written with different casing is not
        // reported as unknown when it would have resolved.
        var known = new HashSet<string>(supplied, EmailParameterComparer.Default);

        return PlaceholderRegex.Matches(text)
            .Select(x => x.Groups[1].Value)
            .Where(x => !known.Contains(x))
            .Distinct(EmailParameterComparer.Default)
            .ToArray();
    }
}
