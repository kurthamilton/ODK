using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterTextsFormSubmitViewModel
{
    [DisplayName(ChapterTextLabels.Description)]
    public string? DescriptionHtml { get; set; }

    [Required]
    [DisplayName(ChapterTextLabels.RegisterText)]
    public string? RegisterMessageHtml { get; set; }

    [DisplayName(ChapterTextLabels.ShortDescription)]
    public string? ShortDescription { get; set; }

    [Required]
    [DisplayName(ChapterTextLabels.WelcomeText)]
    public string? WelcomeMessageHtml { get; set; }
}
