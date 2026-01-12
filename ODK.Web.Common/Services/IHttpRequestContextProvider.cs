using ODK.Core.Web;

namespace ODK.Web.Common.Services;

public interface IHttpRequestContextProvider
{
    IHttpRequestContext Get();
}
