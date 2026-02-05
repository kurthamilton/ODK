using System.Threading.Tasks;
using ODK.Services;

namespace ODK.Web.Common.Services;

public class RequestStoreFactory : IRequestStoreFactory
{
    private readonly IRequestStore _requestStore;

    public RequestStoreFactory(IRequestStore requestStore)
    {
        _requestStore = requestStore;
    }

    public Task<IRequestStore> Create(IServiceRequest request) => _requestStore.Load(request);
}