using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterSubmitViewModel : IChapterImageFormViewModel
{
    [Required]
    [MaxLength(1024)]
    public string? Description { get; set; }

    public string? ImageDataUrl { get; set; }

    public LocationFormViewModel Location { get; set; } = new();

    [Required]
    [MaxLength(100)]
    [Remote("Available", "Groups", ErrorMessage = "That name is already taken")]
    public string? Name { get; set; }

    [Display(Name = "Topics")]
    public List<Guid>? TopicIds { get; set; }
}
