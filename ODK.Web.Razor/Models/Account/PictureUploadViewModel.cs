namespace ODK.Web.Razor.Models.Account;

public class PictureUploadViewModel
{
    /// <summary>
    /// Dummy form field
    /// </summary>
    public string? Image { get; init; }

    public string? ImageDataUrl { get; init; }

    public int? ImageVersion { get; init; }
}