namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterImageFormViewModel : ChapterImageFormSubmitViewModel
{
    /// <summary>
    /// Ratio the crop is locked to, or null to leave it free.
    /// </summary>
    public decimal? AspectRatio { get; set; }

    /// <summary>
    /// The picture already stored, as a data url, so the form opens on what it is replacing.
    /// </summary>
    public string? CurrentImageDataUrl { get; set; }
}
