namespace ODK.Core.Utils;

/// <summary>
/// Matches a value against the wildcard rule syntax used by configured rules - <c>Logging:IgnoreExceptions</c>
/// paths and user agents, and <c>RateLimiting:BlockPaths</c>.
/// </summary>
/// <remarks>
/// One implementation because those rules are one syntax, written by the same person into the same file and
/// expected to behave the same way. It lived twice, and the copies had already drifted: a fix to the bare "*"
/// case landed in one of them and not the other.
/// </remarks>
public static class WildcardUtils
{
    /// <summary>
    /// Whether <paramref name="value"/> satisfies <paramref name="rule"/>. A leading "*" means ends-with, a
    /// trailing "*" means starts-with, both mean contains, and neither means equals. Always case-insensitive.
    /// </summary>
    public static bool Matches(string rule, string value)
    {
        var (wildStart, wildEnd) = (rule.StartsWith('*'), rule.EndsWith('*'));

        if (wildStart && wildEnd)
        {
            /* "*" on its own is both wildcards at once with nothing between them, so there is no substring to
               look for and it matches anything. Taking rule[1..^1] of it asks for a range ending before it
               starts, which throws rather than matching everything. */
            return rule.Length == 1 || value.Contains(rule[1..^1], StringComparison.OrdinalIgnoreCase);
        }

        if (wildStart)
        {
            return value.EndsWith(rule[1..], StringComparison.OrdinalIgnoreCase);
        }

        if (wildEnd)
        {
            return value.StartsWith(rule[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return value.Equals(rule, StringComparison.OrdinalIgnoreCase);
    }
}
