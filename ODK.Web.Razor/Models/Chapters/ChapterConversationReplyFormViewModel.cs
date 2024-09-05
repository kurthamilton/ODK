using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterConversationReplyFormViewModel
{
    [Required]
    public string? Message { get; set; }
}
