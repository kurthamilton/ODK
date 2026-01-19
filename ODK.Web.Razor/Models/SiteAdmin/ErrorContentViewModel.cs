using ODK.Services.Logging;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class ErrorContentViewModel
{
    public ErrorContentViewModel(ErrorDto error)
    {
        Error = error;
    }

    public ErrorDto Error { get; }
}