using System.Text.RegularExpressions;
using System.Web;

namespace ODK.Core.Web;

// https://medium.com/@feldy7k/preventing-xss-attacks-in-net-8-api-with-html-sanitizer-method-0bb04413526b
public class HtmlSanitizer : IHtmlSanitizer
{
    private static readonly Regex HttpLinkRegex = new Regex(@"(http|https):\/\/[^\s<>]+", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JavascriptLinkRegex = new Regex(@"href\s*=\s*['""]javascript:[^'""]*['""]", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly IReadOnlyCollection<string> AttributeBlacklist = 
    [ 
        "onload", "onclick", "onerror", "href", "src" 
    ];

    private static readonly IReadOnlyCollection<string> DefaultTagWhitelist =
    [
        "a", "ul", "li", "b", "i"
    ];

    private static readonly IReadOnlyCollection<string> TagBlacklist = 
    [ 
        "script", "iframe", "object", "embed", "form" 
    ];

    public string Sanitize(string html) => Sanitize(html, DefaultTagWhitelist);

    public string Sanitize(string html, IEnumerable<string> allowedTags)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        // Remove blacklisted tags
        foreach (var tag in TagBlacklist)
        {
            var tagRegex = new Regex($"<\\/?\\s*{tag}\\s*[^>]*>", RegexOptions.IgnoreCase);
            html = tagRegex.Replace(html, (Match match) => HttpUtility.HtmlEncode(match.Value));
        }

        // Remove blacklisted attributes
        foreach (var attr in AttributeBlacklist)
        {
            var attrRegex = new Regex($"{attr}\\s*=\\s*['\"].*?['\"]", RegexOptions.IgnoreCase);
            html = attrRegex.Replace(html, (Match match) => HttpUtility.HtmlEncode(match.Value));
        }

        // Remove javascript: links
        html = JavascriptLinkRegex.Replace(html, (Match match) => HttpUtility.HtmlEncode(match.Value));

        // Remove plain http and https links
        html = HttpLinkRegex.Replace(html, (Match match) => HttpUtility.HtmlEncode(match.Value));

        return html;
    }
}
