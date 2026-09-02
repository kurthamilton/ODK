using ODK.Services.Logging;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class ErrorContentViewModel
{
    public ErrorContentViewModel(ErrorDto error, TimeZoneInfo timeZone)
    {
        Error = error;
        TimeZone = timeZone;
    }

    public ErrorDto Error { get; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public TimeZoneInfo TimeZone { get; }
}