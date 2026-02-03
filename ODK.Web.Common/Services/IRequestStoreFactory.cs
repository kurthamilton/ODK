using System.Threading.Tasks;
using ODK.Services;

namespace ODK.Web.Common.Services;

public interface IRequestStoreFactory
{
    Task<IRequestStore> Create(ServiceRequest request);
}
