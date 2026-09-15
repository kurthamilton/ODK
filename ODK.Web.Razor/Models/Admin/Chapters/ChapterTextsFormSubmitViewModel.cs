using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterTextsFormSubmitViewModel
{
    [Required]
    [DisplayName(ChapterTextLabels.Description)]
    public string? DescriptionHtml { get; set; }

    [Required]
    [DisplayName(ChapterTextLabels.RegisterText)]
    public string? RegisterMessageHtml { get; set; }

    /// <summary>
    /// The line the group is listed by. Required on a platform that shows it and absent from the form
    /// everywhere else, so the requirement is the service's to apply against the platform - see
    /// <see cref="ChapterTexts.ShowsShortDescription"/> and UpdateChapterTexts.
    /// </summary>
    [DisplayName(ChapterTextLabels.ShortDescription)]
    public string? ShortDescription { get; set; }

    [Required]
    [DisplayName(ChapterTextLabels.WelcomeText)]
    public string? WelcomeMessageHtml { get; set; }
}
