using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterQuestionFormViewModel
{
    [Required]
    [DisplayName("Answer")]
    public string? AnswerHtml { get; set; }

    [Required]
    public string? Question { get; set; }
}
