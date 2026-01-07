using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ODK.Web.Common.Validation;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormSubmitViewModel
{
    [DisplayName("Max number of attendees")]
    [Range(1, int.MaxValue, ErrorMessage = "{0} cannot be less than 1")]
    public int? AttendeeLimit { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; } = string.Empty;

    public bool Draft { get; set; }

    [DisplayName("End time")]
    [TimeOfDay]
    public string? EndTime { get; set; }

    public IReadOnlyCollection<SelectListItem> HostOptions { get; set; } = [];

    public List<Guid> Hosts { get; set; } = new();

    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool Public { get; set; }

    [DisplayName("RSVP deadline")]
    public DateTime? RsvpDeadline { get; set; }

    [DisplayName("Cost")]
    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be less than 0")]
    public decimal? TicketCost { get; set; }

    [DisplayName("Deposit")]
    [Range(0, double.MaxValue, ErrorMessage = "{1} cannot be less than 0")]
    public decimal? TicketDepositCost { get; set; }

    public string? Time { get; set; } = string.Empty;

    [Required]
    public Guid Venue { get; set; }
}
