using System.Threading.Tasks;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public interface IOdkRoutesFactory
{
    Task<IOdkRoutes> Create(PlatformType platform);
}