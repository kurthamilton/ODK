using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Web.Common.Validation;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel
{
    [DisplayName("Day of week")]
    public DayOfWeek? DefaultDayOfWeek { get; set; }

    [DisplayName("Default description")]
    public string? DefaultDescription { get; set; }

    [DisplayName("Scheduled email day of week")]    
    public DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }
    
    [DisplayName("Scheduled email time of day")]
    [TimeOfDay]
    public string? DefaultScheduledEmailTimeOfDay { get; set; }

    [DisplayName("Start time")]
    [TimeOfDay]
    public string? DefaultStartTime { get; set; }

    [DisplayName("Disable comments")]
    public bool DisableComments { get; set; }
}
