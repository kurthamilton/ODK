using System.Net;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Errors;

public class ErrorModel : OdkPageModel
{
    public ErrorModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(HttpStatusCode statusCode)
    {
        ErrorStatusCode = statusCode;
    }
}
