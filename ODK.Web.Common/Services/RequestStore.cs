using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Logging;

namespace ODK.Web.Common.Services;

/// <summary>
/// Only to be injected into UI classes, otherwise instances should be created
/// from the <see cref="RequestStoreFactory"/>
/// </summary>
public class RequestStore : IRequestStore
{
    private Chapter? _chapter;
    private ChapterAdminMember? _currentChapterAdminMember;
    private bool _currentChapterAdminMemberLoaded;
    private Member? _currentMember;
    private readonly ILoggingService _loggingService;
    private ServiceRequest? _serviceRequest;
    private readonly IUnitOfWork _unitOfWork;

    private readonly Lazy<ChapterServiceRequest> _chapterServiceRequest;
    private readonly Lazy<Guid> _currentMemberId;
    private readonly Lazy<MemberChapterServiceRequest> _memberChapterServiceRequest;
    private readonly Lazy<MemberServiceRequest> _memberServiceRequest;

    public RequestStore(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;

        _chapterServiceRequest = new(() => ChapterServiceRequest.Create(Chapter, ServiceRequest));
        _currentMemberId = new(
            () => CurrentMemberIdOrDefault ?? throw new OdkNotAuthorizedException());
        _memberChapterServiceRequest = new(() => MemberChapterServiceRequest.Create(Chapter, MemberServiceRequest));
        _memberServiceRequest = new(() => MemberServiceRequest.Create(CurrentMember, ServiceRequest));
    }

    public Chapter Chapter => _chapter ?? throw new OdkNotFoundException();

    public Chapter? ChapterOrDefault => _chapter;

    public ChapterServiceRequest ChapterServiceRequest => _chapterServiceRequest.Value;

    public Member CurrentMember => _currentMember ?? throw new OdkNotAuthenticatedException();

    public Guid CurrentMemberId => _currentMemberId.Value;

    public Guid? CurrentMemberIdOrDefault => ServiceRequest.CurrentMemberIdOrDefault;

    public Member? CurrentMemberOrDefault => _currentMember;

    public bool Loaded { get; private set; }

    public MemberChapterServiceRequest MemberChapterServiceRequest => _memberChapterServiceRequest.Value;

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform { get; private set; }

    public ServiceRequest ServiceRequest => _serviceRequest!;

    public async Task<ChapterAdminMember?> GetCurrentChapterAdminMember()
    {
        if (_currentChapterAdminMemberLoaded)
        {
            return _currentChapterAdminMember;
        }

        _currentChapterAdminMember = await _unitOfWork.ChapterAdminMemberRepository
            .GetByMemberId(Platform, CurrentMemberId, Chapter.Id).Run();
        _currentChapterAdminMemberLoaded = true;
        return _currentChapterAdminMember;
    }

    /// <summary>
    /// Called from middleware and <see cref="RequestStoreFactory"/>
    /// </summary>
    public Task<IRequestStore> Load(ServiceRequest request) => Load(request, verbose: false);

    public void Reset()
    {
        _chapter = null;
        _currentChapterAdminMember = null;
        _currentMember = null;
        Loaded = false;
        _serviceRequest = null;
    }

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(IUnitOfWork unitOfWork, bool verbose)
    {
        var context = ServiceRequest.HttpRequestContext;

        if (Platform == PlatformType.DrunkenKnitwits)
        {
            context.RouteValues.TryGetValue("chapterName", out var chapterName);

            if (!string.IsNullOrEmpty(chapterName))
            {
                chapterName = Chapter.GetFullName(PlatformType.DrunkenKnitwits, chapterName);

                if (verbose)
                {
                    _loggingService.Info($"RequestStore: getting chapter by name: '{chapterName}'");
                }

                return unitOfWork.ChapterRepository.GetByName(Platform, chapterName);
            }
        }
        else
        {
            context.RouteValues.TryGetValue("slug", out var slug);

            if (!string.IsNullOrEmpty(slug))
            {
                if (verbose)
                {
                    _loggingService.Info($"RequestStore: getting chapter by slug: '{slug}'");
                }

                return unitOfWork.ChapterRepository.GetBySlug(Platform, slug);
            }
        }

        var chapterId = context.RouteValues.TryGetValue("chapterId", out var chapterIdRouteValue) &&
            Guid.TryParse(chapterIdRouteValue, out Guid id)
                ? id
                : default(Guid?);

        if (chapterId != null)
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by id: '{chapterId}'");
            }

            return _unitOfWork.ChapterRepository.GetByIdOrDefault(Platform, chapterId.Value);
        }

        if (verbose)
        {
            var message =
                "RequestStore: could not use the request URL to determine chapter";

            var properties = new Dictionary<string, string?>
            {
                { "Url", ServiceRequest.HttpRequestContext.RequestUrl }
            };

            foreach (var routeValue in ServiceRequest.HttpRequestContext.RouteValues)
            {
                properties.Add(routeValue.Key, routeValue.Value);
            }

            _loggingService.Warn(message, properties);
        }

        return new DefaultDeferredQuerySingleOrDefault<Chapter>();
    }

    private async Task<IRequestStore> Load(ServiceRequest serviceRequest, bool verbose)
    {
        if (Loaded)
        {
            return this;
        }

        // Set the platform directly to persist when resetting other state
        _serviceRequest = serviceRequest;
        Platform = serviceRequest.Platform;

        var (chapter, currentMember) = await _unitOfWork.RunAsync(
            x => GetChapterQuery(x, verbose),
            x => CurrentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(CurrentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>());

        _chapter = chapter;
        _currentMember = currentMember;
        Loaded = true;

        return this;
    }
}