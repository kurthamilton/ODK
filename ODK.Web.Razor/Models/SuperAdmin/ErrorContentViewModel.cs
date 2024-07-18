using ODK.Core.Chapters;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class ErrorContentViewModel
{
    public ErrorContentViewModel(Chapter chapter, ErrorDto error)
    {
        Chapter = chapter;
        Error = error;
    }

    public Chapter Chapter { get; }

    public ErrorDto Error { get; }
}
