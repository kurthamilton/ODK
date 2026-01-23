using System.Net;

namespace ODK.Web.Razor.Pages.Groups;

public class ErrorModel : OdkPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(int statusCode)
    {
        ErrorStatusCode = (HttpStatusCode)statusCode;
    }
}