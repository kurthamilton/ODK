namespace ODK.Infrastructure.Settings;

/// <summary>
/// One entry under <c>Logging:IgnoreExceptions</c>, mapped to the service's own rule type in
/// <c>DependencyRegistrar</c>.
/// </summary>
/// <remarks>
/// <para>
/// A near-copy of the service's rule rather than the same type: what binds to configuration lives here, so the
/// service is free to change the shape it works with without that being a change to the config contract, and
/// this project depends on the service rather than the other way about.
/// </para>
/// <para>
/// Every property is nullable because a rule states only the criteria it uses - a property left out of the JSON
/// binds to null, and an entry naming just <c>Exceptions</c> and <c>Headers</c> is a normal way to write one.
/// The mapping turns each null into an empty set, which is what "this rule has no criteria of that kind" means.
/// Declaring them non-null would make the annotation a promise the binder cannot keep.
/// </para>
/// </remarks>
public class LoggingIgnoreExceptionSettings
{
    public required string[]? Exceptions { get; init; }

    /// <summary>
    /// Wildcard rules per header name, e.g. <c>{ "X-Forwarded-For": [ "10.*" ] }</c>.
    /// </summary>
    public required Dictionary<string, string[]>? Headers { get; init; }

    public required string[]? Paths { get; init; }

    public required string[]? PathPatterns { get; init; }

    public required string[]? UserAgents { get; init; }
}
