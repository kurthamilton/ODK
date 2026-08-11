namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailFormViewModel : ChapterEmailFormSubmitViewModel
{
    /// <summary>
    /// The placeholders this template may use, offered as insertable buttons. Render-only, so it sits
    /// here rather than on the submit model - the form posts the content, not the list it was built from.
    /// </summary>
    public required IReadOnlyCollection<string> Placeholders { get; init; }
}
