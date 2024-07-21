using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel
{
    [DisplayName("Day of week")]
    public DayOfWeek? DefaultDayOfWeek { get; set; }

    [DisplayName("Scheduled email day of week")]    
    public DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }
    
    [DisplayName("Scheduled email time of day")]
    [RegularExpression(@"(2[0-3]|[0-1][0-9]):[0-5][0-9]", ErrorMessage = "Time must be in the format 00:00")]
    public string? DefaultScheduledEmailTimeOfDay { get; set; }
}
