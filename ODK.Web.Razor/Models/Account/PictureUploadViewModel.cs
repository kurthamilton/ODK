namespace ODK.Web.Razor.Models.Account;

public class PictureUploadViewModel
{
    public required string ChapterName { get; set; }

    public int? CropHeight { get; set; }

    public int? CropWidth { get; set; }

    public int? CropX { get; set; }

    public int? CropY { get; set; }

    public string? ImageDataUrl { get; set; }
}
