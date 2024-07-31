using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormViewModel
{
    public Guid ChapterId { get; set; }

    public Guid CurrentMemberId { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; } = "";

    public bool Draft { get; set; }

    public IReadOnlyCollection<SelectListItem> HostOptions { get; set; } = [];

    public List<Guid> Hosts { get; set; } = new();

    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    public bool Public { get; set; }

    public string? Time { get; set; } = "";

    [Required]
    public Guid Venue { get; set; }
}
