using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventScheduledEmailFormViewModel
{
    public required Chapter Chapter { get; set; }

    public required Guid EventId { get; set; }

    [DisplayName("Scheduled email date")]
    public required DateTime? ScheduledEmailDate { get; set; }

    [DisplayName("Scheduled email time")]
    [RegularExpression(@"(2[0-3]|[0-1][0-9]):[0-5][0-9]", ErrorMessage = "Time must be in the format 00:00")]
    public required string? ScheduledEmailTime { get; set; }
}
