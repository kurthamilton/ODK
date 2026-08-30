using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterTextsFormViewModel
{
    [DisplayName("Description")]
    public string? DescriptionHtml { get; set; }

    [Required]
    [DisplayName("Message to non-members on the registration page")]
    public string? RegisterMessageHtml { get; set; }

    [DisplayName("Short description")]
    public string? ShortDescription { get; set; }

    [Required]
    [DisplayName("Message to non-members on the home page")]
    public string? WelcomeMessageHtml { get; set; }
}