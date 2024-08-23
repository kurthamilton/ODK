using System.ComponentModel;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventScheduledEmailFormViewModel
{
    public required Chapter Chapter { get; set; }

    public required Guid EventId { get; set; }

    [DisplayName("Scheduled email date")]
    public required DateTime? ScheduledEmailDate { get; set; }
}
