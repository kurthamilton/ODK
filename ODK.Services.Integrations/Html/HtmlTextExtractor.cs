using AngleSharp.Html.Parser;
using ODK.Core.Utils;
using ODK.Services.Html;

namespace ODK.Services.Integrations.Html;

/// <summary>
/// Extracts plain text from rich text by parsing it.
/// </summary>
/// <remarks>
/// Parsed rather than pattern-matched, for the same reason <see cref="HtmlValidator"/> is: a regex over
/// tags gets entities wrong, keeps the contents of a script element, and can be walked past by markup
/// spaced to dodge it.
/// </remarks>
public class HtmlTextExtractor : IHtmlTextExtractor
{
    /// <summary>
    /// Elements whose boundary is a word boundary. A text node carries no trace of the element it came
    /// from, so without a separator inserted here "&lt;p&gt;One&lt;/p&gt;&lt;p&gt;Two&lt;/p&gt;" reads
    /// back as "OneTwo". Inline elements are deliberately absent - "Un&lt;span&gt;split&lt;/span&gt;able"
    /// is one word and has to stay one.
    /// </summary>
    private const string SeparatedSelector =
        "address, blockquote, br, div, h1, h2, h3, h4, h5, h6, hr, li, ol, p, pre, table, td, th, tr, ul";

    private static readonly HtmlParser Parser = new();

    public string ToPlainText(string? html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        // Parsed inside a body so a fragment is treated as content rather than as a whole document.
        var document = Parser.ParseDocument($"<html><body>{html}</body></html>");

        /* Removed rather than skipped over: the text of a script or a style element is code, and it is
           part of TextContent like any other text node. Nothing the editor produces contains either, but
           this reads values stored before that was enforced. */
        foreach (var element in document.QuerySelectorAll("script, style").ToArray())
        {
            element.Remove();
        }

        foreach (var element in document.Body!.QuerySelectorAll(SeparatedSelector).ToArray())
        {
            element.After(document.CreateTextNode(" "));
        }

        // Collapses the separators inserted above along with any whitespace the author left, and covers
        // the non-breaking space that &nbsp; decodes to.
        return document.Body.TextContent.NormaliseWhitespace();
    }
}
