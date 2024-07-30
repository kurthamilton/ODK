using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailSettingsFormViewModel
{
    [Required]
    [DisplayName("Address")]
    [EmailAddress]
    public string FromAddress { get; set; } = "";

    [Required]
    [DisplayName("Name")]
    public string FromName { get; set; } = "";
}
