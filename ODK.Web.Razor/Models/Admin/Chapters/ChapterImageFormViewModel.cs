namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterImageFormViewModel : ChapterImageFormSubmitViewModel
{
    /// <summary>
    /// Ratio the crop is locked to, or null to leave it free.
    /// </summary>
    public decimal? AspectRatio { get; set; }
}
