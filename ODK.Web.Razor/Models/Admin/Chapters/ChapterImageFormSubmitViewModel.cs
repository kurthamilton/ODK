namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterImageFormSubmitViewModel
{
    /// <summary>
    /// The file input the member picks a picture with. Its value is never read - the picture reaches the
    /// server as <see cref="ImageDataUrl"/>, written by the cropper.
    /// </summary>
    public string? Image { get; set; }

    public string? ImageDataUrl { get; set; }
}
