using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterTextsFormViewModel
{
    public string? Description { get; set; }

    [Required]
    [DisplayName("Message to non-members on the registration page")]
    public string? RegisterMessage { get; set; }

    [DisplayName("Short description")]
    public string? ShortDescription { get; set; }

    [Required]
    [DisplayName("Message to non-members on the home page")]
    public string? WelcomeMessage { get; set; }
}