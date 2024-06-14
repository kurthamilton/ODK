using ODK.Core.Chapters;
using ODK.Core.Logging;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class ErrorContentViewModel
{
    public ErrorContentViewModel(Chapter chapter, LogMessage error)
    {
        Chapter = chapter;
        Error = error;
    }

    public Chapter Chapter { get; }

    public LogMessage Error { get; }
}
