namespace ODK.Core.Web;

public class HtmlSanitizerOptions
{
    public bool AllowLinks { get; init; }

    public IReadOnlyCollection<string>? AttributeWhitelist { get; init; }

    public IReadOnlyCollection<string>? TagWhitelist { get; init; }
}
