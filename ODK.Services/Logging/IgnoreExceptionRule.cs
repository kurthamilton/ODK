namespace ODK.Services.Logging;

/// <summary>
/// A rule that suppresses error logging for one or more exception types on matching requests. An exception
/// is ignored when its short type name is in <see cref="Exceptions"/> and the request matches at least one
/// of <see cref="Paths"/> (wildcard rules), <see cref="PathPatterns"/> (regex) or <see cref="UserAgents"/>
/// (wildcard rules). A rule with no path/pattern/user-agent matches nothing - use "*" to match everything.
/// </summary>
public class IgnoreExceptionRule
{
    public string[] Exceptions { get; init; } = [];

    public string[] Paths { get; init; } = [];

    public string[] PathPatterns { get; init; } = [];

    public string[] UserAgents { get; init; } = [];
}
