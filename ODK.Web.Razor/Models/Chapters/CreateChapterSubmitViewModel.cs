using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterSubmitViewModel
{
    [Required]
    [MaxLength(1024)]
    public string? Description { get; set; }

    public LocationFormViewModel Location { get; set; } = new();

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Display(Name = "Topics")]
    public List<Guid> TopicIds { get; set; } = new();
}
