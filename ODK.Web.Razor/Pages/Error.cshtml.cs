using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : OdkPageModel
{
    public HttpStatusCode ErrorStatusCode { get; private set; }

    public void OnGet(int statusCode)
    {
        ErrorStatusCode = (HttpStatusCode)statusCode;
    }
}