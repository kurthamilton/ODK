using System.ComponentModel;
using ODK.Web.Common.Validation;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormSubmitViewModel
{
    [DisplayName("Day of week")]
    public DayOfWeek? DefaultDayOfWeek { get; set; }

    [DisplayName("Default description")]
    public string? DefaultDescription { get; set; }

    [DisplayName("End time")]
    [TimeOfDay]
    public string? DefaultEndTime { get; set; }

    [DisplayName("Day of week")]
    public DayOfWeek? DefaultScheduledEmailDayOfWeek { get; set; }

    [DisplayName("Time of day")]
    [TimeOfDay]
    public string? DefaultScheduledEmailTimeOfDay { get; set; }

    [DisplayName("Start time")]
    [TimeOfDay]
    public string? DefaultStartTime { get; set; }

    [DisplayName("Disable comments")]
    public bool DisableComments { get; set; }
}
