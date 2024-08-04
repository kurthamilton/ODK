using System.Net;

namespace ODK.Web.Razor.Pages.Errors;

public class ErrorModel : OdkPageModel2
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(HttpStatusCode statusCode)
    {
        ErrorStatusCode = statusCode;
    }
}
