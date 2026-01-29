using System.Threading.Tasks;
using ODK.Services;
using ODK.Web.Common.Services;

namespace ODK.Web.Common.Routes;

public class OdkRoutesFactory : IOdkRoutesFactory
{
    private IOdkRoutes? _odkRoutes;
    private readonly IRequestStoreFactory _requestStoreFactory;

    public OdkRoutesFactory(IRequestStoreFactory requestStoreFactory)
    {
        _requestStoreFactory = requestStoreFactory;
    }

    public async Task<IOdkRoutes> Create(ServiceRequest request)
    {
        if (_odkRoutes != null)
        {
            return _odkRoutes;
        }

        var requestStore = await _requestStoreFactory.Create(request);
        _odkRoutes = new OdkRoutes(requestStore);
        return _odkRoutes;
    }
}
