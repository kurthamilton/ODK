using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterMessageReplyFormViewModel
{
    [Required]
    [DisplayName("Message")]
    public string? MessageHtml { get; set; }
}
