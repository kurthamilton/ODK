namespace ODK.Web.Razor.Models.Admin.Chapters;

/// <summary>
/// An image a group admin has stored, shown read-only, with the upload form behind a modal.
/// </summary>
public class ChapterAdminImageViewModel
{
    /// <summary>
    /// Ratio the crop is locked to, or null to leave it free.
    /// </summary>
    public decimal? AspectRatio { get; init; }

    /// <summary>
    /// Url the upload form posts to.
    /// </summary>
    public required string FormAction { get; init; }

    /// <summary>
    /// Distinguishes this component's modal from any other on the page.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// What the image shows, for a reader who cannot see it.
    /// </summary>
    public required string ImageAlt { get; init; }

    public string? ImageClass { get; init; }

    /// <summary>
    /// Url of the image already stored, or null when there is none.
    /// </summary>
    public required string? ImageUrl { get; init; }

    public required string ModalTitle { get; init; }
}
