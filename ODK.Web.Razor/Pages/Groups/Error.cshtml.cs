using System.Net;

namespace ODK.Web.Razor.Pages.Groups;

public class ErrorModel : OdkGroupPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(HttpStatusCode statusCode)
    {
        ErrorStatusCode = statusCode;
    }
}
