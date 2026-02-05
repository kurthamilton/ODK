using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ChapterServiceRequestImpl = ODK.Services.ChapterServiceRequest;
using MemberChapterServiceRequestImpl = ODK.Services.MemberChapterServiceRequest;
using MemberServiceRequestImpl = ODK.Services.MemberServiceRequest;

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
    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private IServiceRequest? _serviceRequest;
    private readonly IUnitOfWork _unitOfWork;

    private readonly Lazy<IChapterServiceRequest> _chapterServiceRequest;
    private readonly Lazy<IMemberChapterServiceRequest> _memberChapterServiceRequest;
    private readonly Lazy<IMemberServiceRequest> _memberServiceRequest;

    public RequestStore(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IPlatformProvider platformProvider)
    {
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;

        _chapterServiceRequest = new(() => ChapterServiceRequestImpl.Create(Chapter, ServiceRequest));
        _memberChapterServiceRequest = new(() => MemberChapterServiceRequestImpl.Create(Chapter, MemberServiceRequest));
        _memberServiceRequest = new(() => MemberServiceRequestImpl.Create(CurrentMember, ServiceRequest));
    }

    public Chapter Chapter => _chapter ?? throw new OdkNotFoundException();

    public Chapter? ChapterOrDefault => _chapter;

    public IChapterServiceRequest ChapterServiceRequest => _chapterServiceRequest.Value;

    public Member CurrentMember => ServiceRequest.CurrentMemberOrDefault ?? throw new OdkNotAuthenticatedException();

    public Member? CurrentMemberOrDefault => ServiceRequest.CurrentMemberOrDefault;

    public bool Loaded { get; private set; }

    public IMemberChapterServiceRequest MemberChapterServiceRequest => _memberChapterServiceRequest.Value;

    public IMemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform { get; private set; }

    public IServiceRequest ServiceRequest => _serviceRequest!;

    public async Task<ChapterAdminMember?> GetCurrentChapterAdminMember()
    {
        if (_currentChapterAdminMemberLoaded)
        {
            return _currentChapterAdminMember;
        }

        _currentChapterAdminMember = await _unitOfWork.ChapterAdminMemberRepository
            .GetByMemberId(Platform, CurrentMember.Id, Chapter.Id).Run();
        _currentChapterAdminMemberLoaded = true;
        return _currentChapterAdminMember;
    }

    /// <summary>
    /// Called from middleware and <see cref="RequestStoreFactory"/>
    /// </summary>
    public Task<IRequestStore> Load(IHttpRequestContext context, Guid? currentMemberIdOrDefault)
        => Load(context, currentMemberIdOrDefault, verbose: false);

    public void Reset()
    {
        _chapter = null;
        _currentChapterAdminMember = null;
        _serviceRequest = null;
        Loaded = false;
    }

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(
        IHttpRequestContext context,
        IUnitOfWork unitOfWork,
        bool verbose)
    {
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

    private async Task<IRequestStore> Load(IHttpRequestContext context, Guid? currentMemberIdOrDefault, bool verbose)
    {
        if (Loaded)
        {
            return this;
        }

        // Set the platform directly to persist when resetting other state
        Platform = _platformProvider.GetPlatform(context.RequestUrl);

        var (chapter, currentMember) = await _unitOfWork.RunAsync(
            x => GetChapterQuery(context, x, verbose),
            x => currentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>());

        _chapter = chapter;
        _serviceRequest = new ServiceRequest
        {
            CurrentMemberOrDefault = currentMember,
            HttpRequestContext = context,
            Platform = Platform
        };
        Loaded = true;

        return this;
    }
}