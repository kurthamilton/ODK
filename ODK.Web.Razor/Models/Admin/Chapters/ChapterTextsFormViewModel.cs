using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterTextsFormViewModel : ChapterTextsFormSubmitViewModel
{
    /// <summary>
    /// Whether to offer the short description box - see <see cref="ChapterTexts.ShowsShortDescription"/>.
    /// Render-only, since what the form asks for is not something a post gets to state.
    /// </summary>
    public required bool ShowShortDescription { get; init; }

    /// <summary>
    /// Endpoint each rich text field posts its content to for the markup checks that cannot run in the
    /// browser - they are a parse and an allow-list the server owns. See the htmlcontent provider in
    /// odk.forms.js.
    /// </summary>
    public required string ValidateUrl { get; init; }
}
