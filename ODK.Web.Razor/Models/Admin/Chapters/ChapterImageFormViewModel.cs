using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterImageFormViewModel
{
    public ChapterImage? ChapterImage { get; set; }
    

    /// <summary>
    /// Dummy form field
    /// </summary>
    public string? Image { get; set; }    

    public string? ImageDataUrl { get; set; }

    public bool Required { get; set; }
}
