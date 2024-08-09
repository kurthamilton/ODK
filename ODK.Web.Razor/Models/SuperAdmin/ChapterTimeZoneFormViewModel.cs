using System.ComponentModel;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class ChapterTimeZoneFormViewModel
{
    [DisplayName("Time zone")]
    public string? TimeZone { get; set; }
}
