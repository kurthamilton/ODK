using System.Threading.Tasks;
using ODK.Services;
using ODK.Services.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProviderFactory : IUrlProviderFactory
{
    private readonly IOdkRoutesFactory _odkRoutesFactory;

    public UrlProviderFactory(IOdkRoutesFactory odkRoutesFactory)
    {
        _odkRoutesFactory = odkRoutesFactory;
    }

    public async Task<IUrlProvider> Create(IServiceRequest request)
    {
        var odkRoutes = await _odkRoutesFactory.Create(request);

        return new UrlProvider(request, odkRoutes);
    }
}