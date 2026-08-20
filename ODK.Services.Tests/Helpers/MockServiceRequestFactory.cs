using System;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Tasks;

namespace ODK.Services.Tests.Helpers;

/// <summary>
/// Rebuilds a request from a job the way the real factory does - by loading the member and chapter the job
/// names out of the context - so a test that runs a job through <see cref="MockBackgroundTaskService"/>
/// exercises the reload rather than being handed the objects the enqueueing code already had.
/// </summary>
internal class MockServiceRequestFactory : IServiceRequestFactory
{
    private const string BaseUrl = "https://example.com";

    private readonly MockOdkContext _context;

    // Defaulted the way MockUnitOfWorkFactory does, so a test that passes no context still gets a factory
    // that behaves - it finds nothing, which is what an empty context holds.
    internal MockServiceRequestFactory(MockOdkContext? context = null)
    {
        _context = context ?? new MockOdkContext();
    }

    public Task<IServiceRequest> Create(JobRequest request)
        => Task.FromResult(CreateServiceRequest(request));

    public Task<IChapterServiceRequest> CreateChapterRequest(JobRequest request)
    {
        var chapter = OdkAssertions.Exists(
            _context.Set<Chapter>().SingleOrDefault(x => x.Id == request.ChapterId));

        return Task.FromResult<IChapterServiceRequest>(
            ChapterServiceRequest.Create(chapter, CreateServiceRequest(request)));
    }

    private IServiceRequest CreateServiceRequest(JobRequest request) => new ServiceRequest
    {
        CurrentMemberOrDefault = request.CurrentMemberId != null
            ? _context.Set<Member>().SingleOrDefault(x => x.Id == request.CurrentMemberId)
            : null,
        HttpRequestContext = new JobHttpRequestContext { BaseUrl = BaseUrl },
        Platform = request.Platform
    };
}
