using System.Threading.Tasks;
using ODK.Services;

namespace ODK.Web.Common.Routes;

public interface IOdkRoutesFactory
{
    Task<IOdkRoutes> Create(ServiceRequest request);
}
