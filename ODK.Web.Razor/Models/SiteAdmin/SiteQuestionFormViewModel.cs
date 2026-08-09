using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteQuestionFormViewModel
{
    [Required]
    public string Answer { get; set; } = string.Empty;

    [Required]
    [DisplayName("Question")]
    public string Question { get; set; } = string.Empty;
}
