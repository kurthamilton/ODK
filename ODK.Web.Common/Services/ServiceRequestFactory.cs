using System.Threading.Tasks;
using ODK.Core;
using ODK.Services;
using ODK.Services.Tasks;

namespace ODK.Web.Common.Services;

public class ServiceRequestFactory : IServiceRequestFactory
{
    private readonly IRequestStore _requestStore;

    public ServiceRequestFactory(IRequestStore requestStore)
    {
        _requestStore = requestStore;
    }

    /* Loading the store rather than building a request directly, because everything downstream of a job -
       IUrlProviderFactory, and IOdkRoutesFactory below it - resolves its own store and finds this one already
       loaded. A request built on the side would leave that path to load a second, empty one. */
    public async Task<IServiceRequest> Create(JobRequest request)
    {
        var requestStore = await _requestStore.Load(request);
        return requestStore.ServiceRequest;
    }

    public async Task<IChapterServiceRequest> CreateChapterRequest(JobRequest request)
    {
        var requestStore = await _requestStore.Load(request);

        // Asserted here so a job about a group that has since been deleted fails naming it, rather than on
        // a store property that reads as though no group was ever asked for.
        OdkAssertions.Exists(
            requestStore.ChapterOrDefault,
            $"Chapter {request.ChapterId} not found for background job");

        return requestStore.ChapterServiceRequest;
    }
}
