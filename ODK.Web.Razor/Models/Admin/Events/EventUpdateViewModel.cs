using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventUpdateViewModel
{

    [Required]
    public string? Body { get; set; }

    public required Chapter Chapter { get; set; }

    public Guid EventId { get; set; }

    [DisplayName("Response status")]
    [Required]
    [MinLength(1)]
    public List<EventResponseType> ResponseTypes { get; set; } = new();

    [Required]
    public string? Subject { get; set; }
}
