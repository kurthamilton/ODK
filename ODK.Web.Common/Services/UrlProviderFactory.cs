using ODK.Services;
using ODK.Services.Web;

namespace ODK.Web.Common.Services;

public class UrlProviderFactory : IUrlProviderFactory
{
    public IUrlProvider Create(ServiceRequest request) => new UrlProvider(request);
}
