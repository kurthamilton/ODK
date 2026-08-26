using System.Threading.Tasks;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class OdkRoutesFactory : IOdkRoutesFactory
{
    private IOdkRoutes? _odkRoutes;

    public OdkRoutesFactory()
    {
    }

    public Task<IOdkRoutes> Create(PlatformType platform)
    {
        if (_odkRoutes != null)
        {
            return Task.FromResult(_odkRoutes);
        }

        _odkRoutes = new OdkRoutes(platform);
        return Task.FromResult(_odkRoutes);
    }
}