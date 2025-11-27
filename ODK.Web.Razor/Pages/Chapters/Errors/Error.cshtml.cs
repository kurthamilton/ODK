using System.Net;

namespace ODK.Web.Razor.Pages.Chapters.Errors;

public class ErrorModel : ChapterPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(HttpStatusCode statusCode)
    {
        ErrorStatusCode = statusCode;
    }
}
