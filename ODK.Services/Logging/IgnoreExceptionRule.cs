namespace ODK.Services.Logging;

/// <summary>
/// A rule that suppresses error logging for one or more exception types on matching requests. An exception
/// is ignored when its short type name is in <see cref="Exceptions"/> and the request matches at least one
/// of <see cref="Paths"/> (wildcard rules), <see cref="PathPatterns"/> (regex), <see cref="UserAgents"/>
/// (wildcard rules) or <see cref="Headers"/> (wildcard rules per header). A rule with none of those matches
/// nothing - use "*" to match everything.
/// </summary>
public class IgnoreExceptionRule
{
    public string[] Exceptions { get; init; } = [];

    /// <summary>
    /// Wildcard rules per header name, e.g. <c>{ "X-Forwarded-For": [ "10.*" ] }</c>. One header matching one
    /// of its values is enough, in keeping with the rest of the rule: listing several headers widens what the
    /// rule catches rather than narrowing it.
    /// </summary>
    /// <remarks>
    /// Names are matched case-insensitively, and so are values - the same wildcard matcher as the other rules,
    /// so "*" at the start means ends-with, at the end means starts-with, and at both means contains.
    /// </remarks>
    public Dictionary<string, string[]> Headers { get; init; } = [];

    public string[] Paths { get; init; } = [];

    public string[] PathPatterns { get; init; } = [];

    public string[] UserAgents { get; init; } = [];
}
