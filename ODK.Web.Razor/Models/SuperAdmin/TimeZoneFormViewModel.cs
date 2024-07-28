using System.ComponentModel;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class TimeZoneFormViewModel
{
    [DisplayName("Time zone")]
    public string? TimeZone { get; set; }
}
