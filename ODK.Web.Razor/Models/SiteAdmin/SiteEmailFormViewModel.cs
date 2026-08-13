namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteEmailFormViewModel : SiteEmailFormSubmitViewModel
{
    /// <summary>The cap for an ordinary template, which is a fragment rather than a whole document.</summary>
    public const int DefaultMaxEditorLines = 30;

    /// <summary>
    /// How tall the editor may grow, in lines, before it scrolls internally instead. Null to grow to
    /// whatever the content needs - which the layout wants, being a whole HTML document that a cap would
    /// leave scrolling inside a box while the buttons below it sit far down the page.
    /// </summary>
    public required int? MaxEditorLines { get; init; }

    /// <summary>
    /// The placeholders this template may use, offered as insertable buttons and accepted by the editor's
    /// check. Render-only, so it sits here rather than on the submit model.
    /// </summary>
    public required IReadOnlyCollection<string> Placeholders { get; init; }

    /// <summary>
    /// Endpoint the editor posts the content to for the HTML checks that cannot run in the browser.
    /// </summary>
    public required string ValidateUrl { get; init; }
}
