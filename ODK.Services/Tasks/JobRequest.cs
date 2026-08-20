using ODK.Core.Platforms;

namespace ODK.Services.Tasks;

/// <summary>
/// What a background job is given in place of an <see cref="IServiceRequest"/>: the resolved answers a job
/// needs, and nothing else. <see cref="IServiceRequestFactory"/> turns one back into a request at the top of
/// the job.
/// </summary>
/// <remarks>
/// <para>
/// The queue is a persistence boundary the compiler cannot see - a job serialised by one deploy is
/// deserialised by the next - so every type reachable from a job argument is a published wire format. This
/// one is primitives only and references neither an entity nor a web type, so no class edited day to day can
/// invalidate a queued job. Adding a property is a change to that format: add only what a job cannot look up
/// for itself, and expect to update the payload test deliberately.
/// </para>
/// <para>
/// Ids rather than the things themselves, so a job runs against current state rather than a snapshot taken
/// when it was queued - a scheduled email may sit for weeks, by which time the member may have been renamed,
/// changed timezone or left the group.
/// </para>
/// </remarks>
public sealed class JobRequest
{
    /// <summary>The site a job builds its URLs against. A job has no request to derive one from.</summary>
    public required string BaseUrl { get; init; }

    public required Guid? ChapterId { get; init; }

    public required Guid? CurrentMemberId { get; init; }

    /// <summary>
    /// Carried rather than re-derived from a URL: the platform a job belongs to is decided when it is queued
    /// and must not change because routing configuration moved while the job sat in the queue.
    /// </summary>
    public required PlatformType Platform { get; init; }

    /* One method rather than an overload per request interface. The chapter is read through a type test
       because every caller holds an IServiceRequest of some kind and only some of them have a chapter -
       overloads would leave the caller to pick, and picking wrong drops the chapter silently. */
    public static JobRequest Create(IServiceRequest request) => new()
    {
        BaseUrl = request.HttpRequestContext.BaseUrl,
        ChapterId = (request as IChapterServiceRequest)?.Chapter.Id,
        CurrentMemberId = request.CurrentMemberIdOrDefault,
        Platform = request.Platform
    };
}
