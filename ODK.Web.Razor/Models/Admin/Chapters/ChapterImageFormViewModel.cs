namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterImageFormViewModel : ChapterImageFormSubmitViewModel
{
    /// <summary>
    /// Ratio the crop is locked to, or null to leave it free.
    /// </summary>
    public decimal? AspectRatio { get; set; }

    /// <summary>
    /// Distinguishes this form's fields from those of another copy of it on the same page. The field
    /// names stay the property names, so only the ids the helpers derive from them need it.
    /// </summary>
    public required string Id { get; init; }
}
