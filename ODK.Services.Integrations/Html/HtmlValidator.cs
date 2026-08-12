using AngleSharp.Html.Parser;
using Ganss.Xss;
using ODK.Services.Html;

namespace ODK.Services.Integrations.Html;

/// <summary>
/// Validates rich text against an allow-list of what the TinyMCE toolbar can produce.
/// </summary>
/// <remarks>
/// Parsed rather than pattern-matched, so a construct spaced to dodge a regex - &lt;img/**/onmouseover=…&gt;
/// - is seen for what it is. The sanitiser is run only to find out what it would remove: nothing is
/// written back, so the stored markup is always exactly what the admin typed.
/// </remarks>
public class HtmlValidator : IHtmlValidator
{
    /// <summary>
    /// What the TinyMCE toolbar can produce: block elements, bold and italic, lists, links and tables.
    /// The editor is the only way these fields are written, so anything outside this was pasted from
    /// elsewhere or hand-crafted.
    /// </summary>
    private static readonly string[] DefaultAllowedTags =
    [
        "a", "b", "blockquote", "br", "code", "em", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "i", "li",
        "ol", "p", "pre", "strong", "sub", "sup", "table", "tbody", "td", "tfoot", "th", "thead", "tr",
        "u", "ul"
    ];

    /// <summary>
    /// No style attribute: the editor is configured with valid_styles set to nothing, so inline styles
    /// never survive it, and accepting them would only admit markup pasted from elsewhere.
    /// </summary>
    private static readonly string[] DefaultAllowedAttributes =
    [
        "colspan", "href", "rowspan", "scope", "title"
    ];

    private static readonly string[] LinkSchemes = ["http", "https", "mailto"];

    public ServiceResult Validate(string? html) => Validate(html, new HtmlValidatorOptions
    {
        AllowLinks = true
    });

    public ServiceResult Validate(string? html, HtmlValidatorOptions options)
    {
        if (string.IsNullOrEmpty(html))
        {
            return ServiceResult.Successful();
        }

        if (options.RequireWellFormed)
        {
            var wellFormed = ValidateWellFormed(html);
            if (!wellFormed.Success)
            {
                return wellFormed;
            }
        }

        var rejected = new List<string>();

        var sanitizer = Create(options);

        // Only tags and attributes are reported. The sanitiser also drops comments, which no admin
        // meant as content and which failing on would only be obstructive.
        sanitizer.RemovingTag += (_, e) => Add(rejected, $"<{e.Tag.NodeName.ToLowerInvariant()}>");
        sanitizer.RemovingAttribute += (_, e) => Add(rejected, e.Attribute.Name.ToLowerInvariant());

        sanitizer.Sanitize(html);

        return rejected.Count > 0
            ? ServiceResult.Failure($"Unsupported HTML: {string.Join(", ", rejected)}")
            : ServiceResult.Successful();
    }

    /* Parsed as a fragment inside a body, not as a document. Strict-mode ParseDocument counts the
       implied html/head/body around a fragment as parse errors, so every template would fail it. In a
       body context only the author's own errors are left - and HTML5's optional end tags still apply,
       so an unclosed <p> or <li> passes, which is correct: those are legal, not mistakes. */
    private static ServiceResult ValidateWellFormed(string html)
    {
        var context = new HtmlParser().ParseDocument("<html><body></body></html>");

        try
        {
            new HtmlParser(new HtmlParserOptions { IsStrictMode = true })
                .ParseFragment(html, context.Body!);

            return ServiceResult.Successful();
        }
        catch (HtmlParseException e)
        {
            return ServiceResult.Failure($"Malformed HTML at line {e.Position.Line}: {e.Message}");
        }
    }

    private static void Add(List<string> rejected, string name)
    {
        // The same offending tag repeated throughout a document is one thing to fix, not twenty.
        if (!rejected.Contains(name))
        {
            rejected.Add(name);
        }
    }

    private static HtmlSanitizer Create(HtmlValidatorOptions options)
    {
        var allowedTags = options.TagWhitelist ?? DefaultAllowedTags;
        var allowedAttributes = options.AttributeWhitelist ?? DefaultAllowedAttributes;

        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(allowedTags.Where(x => options.AllowLinks || x != "a"));

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(allowedAttributes);

        // Cleared rather than trimmed: with no style attribute allowed there is nothing for a CSS
        // property list to apply to, and leaving the defaults in place would only mislead.
        sanitizer.AllowedCssProperties.Clear();

        sanitizer.AllowedSchemes.Clear();
        if (options.AllowLinks)
        {
            sanitizer.AllowedSchemes.UnionWith(LinkSchemes);
        }

        return sanitizer;
    }
}
