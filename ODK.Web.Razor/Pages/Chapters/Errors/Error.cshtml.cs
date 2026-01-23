using System.Net;

namespace ODK.Web.Razor.Pages.Chapters.Errors;

public class ErrorModel : OdkPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(int statusCode)
    {
        ErrorStatusCode = (HttpStatusCode)statusCode;
    }
}