namespace ODK.Core.Web;

public class HtmlSanitizer : IHtmlSanitizer
{
    private static readonly IReadOnlyCollection<string> DefaultTags =
    [
        "a", "ul", "li", "b", "i"
    ];

    public string Encode(string html) => Encode(html, DefaultTags);

    public string Encode(string html, IEnumerable<string> allowedTags)
    {
        return html;
    }
}
