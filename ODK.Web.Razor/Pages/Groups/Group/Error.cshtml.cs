using System.Net;

namespace ODK.Web.Razor.Pages.Groups.Group;

public class ErrorModel : OdkGroupPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(int statusCode)
    {
        ErrorStatusCode = (HttpStatusCode)statusCode;
    }
}