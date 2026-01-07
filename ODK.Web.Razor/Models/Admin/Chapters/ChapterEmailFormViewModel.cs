using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailFormViewModel
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;
}
