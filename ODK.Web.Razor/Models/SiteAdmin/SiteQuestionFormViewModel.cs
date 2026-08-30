using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteQuestionFormViewModel
{
    [Required]
    [DisplayName("Answer")]
    public string AnswerHtml { get; set; } = string.Empty;

    [Required]
    [DisplayName("Question")]
    public string Question { get; set; } = string.Empty;
}
