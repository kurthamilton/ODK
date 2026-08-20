using ODK.Services.Tasks;

namespace ODK.Services;

/// <summary>
/// Rebuilds the request context a background job runs under, from the ids its <see cref="JobRequest"/>
/// carries.
/// </summary>
/// <remarks>
/// Declared here and implemented in the web layer, the same way <see cref="Web.IUrlProviderFactory"/> is: a
/// job lives in this project and the request store it loads does not.
/// </remarks>
public interface IServiceRequestFactory
{
    /// <summary>
    /// The request the job runs under. Its current member is loaded from
    /// <see cref="JobRequest.CurrentMemberId"/>, and is null where the job names none or the account has since
    /// been deleted.
    /// </summary>
    Task<IServiceRequest> Create(JobRequest request);

    /// <summary>
    /// The same, for a job whose work is about one group.
    /// </summary>
    /// <exception cref="Core.Exceptions.OdkNotFoundException">
    /// The chapter named by <see cref="JobRequest.ChapterId"/> no longer exists, or the job named none. A job
    /// about a group that has been deleted has nothing to do, and saying so beats running against a ghost.
    /// </exception>
    Task<IChapterServiceRequest> CreateChapterRequest(JobRequest request);
}
