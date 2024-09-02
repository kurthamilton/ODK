using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterMessageReplyFormViewModel
{
    [Required]
    public string? Message { get; set; }
}
