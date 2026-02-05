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

    public Task<IRequestStore> Create(IServiceRequest request)
        // Only does a full load if the request store isn't already loaded
        // Allows for lazy-loading when running outside the request pipeline
        => _requestStore.Load(request.HttpRequestContext, request.CurrentMemberIdOrDefault);
}