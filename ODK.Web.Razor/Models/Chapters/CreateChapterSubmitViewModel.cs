using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ODK.Web.Razor.Models.Account;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterSubmitViewModel : LocationPickerViewModel, IChapterImageFormViewModel
{
    public string? ImageDataUrl { get; set; }

    [Required]
    [MaxLength(100)]
    [Remote("Available", "Groups", ErrorMessage = "That name is already taken")]
    public string? Name { get; set; }

    [Display(Name = "Topics")]
    public List<Guid>? TopicIds { get; set; }
}