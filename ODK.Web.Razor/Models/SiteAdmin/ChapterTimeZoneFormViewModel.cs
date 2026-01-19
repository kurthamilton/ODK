using System.ComponentModel;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class ChapterTimeZoneFormViewModel
{
    [DisplayName("Time zone")]
    public string? TimeZone { get; set; }
}