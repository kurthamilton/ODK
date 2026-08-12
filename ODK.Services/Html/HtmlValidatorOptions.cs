namespace ODK.Services.Html;

public class HtmlValidatorOptions
{
    public bool AllowLinks { get; init; }

    /// <summary>
    /// Replaces the default set rather than adding to it. Null keeps the default.
    /// </summary>
    public IReadOnlyCollection<string>? AttributeWhitelist { get; init; }

    /// <summary>
    /// Also reject markup an HTML5 parser would recover from rather than silently fixing it - a tag
    /// left open at the end of the input, a tag closed out of order. Off by default: recovery is the
    /// spec's behaviour and the result usually renders, so this is for content an author hand-writes
    /// and would rather be told about.
    /// </summary>
    public bool RequireWellFormed { get; init; }

    /// <inheritdoc cref="AttributeWhitelist" />
    public IReadOnlyCollection<string>? TagWhitelist { get; init; }
}
