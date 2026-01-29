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
using ODK.Web.Common.Extensions;

namespace ODK.Web.Common.Services;

/// <summary>
/// Only to be injected into UI classes, otherwise instances should be created
/// from the <see cref="RequestStoreFactory"/>
/// </summary>
public class RequestStore : IRequestStore
{
    private Chapter? _chapter;
    private ChapterAdminMember? _currentChapterAdminMember;
    private Member? _currentMember;
    private readonly ILoggingService _loggingService;
    private bool _loaded;
    private ServiceRequest? _serviceRequest;
    private readonly IUnitOfWork _unitOfWork;

    private readonly Lazy<Guid> _currentMemberId;
    private readonly Lazy<MemberChapterServiceRequest> _memberChapterServiceRequest;
    private readonly Lazy<MemberServiceRequest> _memberServiceRequest;

    public RequestStore(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService)
    {        
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;

        _currentMemberId = new(
            () => CurrentMemberIdOrDefault ?? throw new OdkNotAuthorizedException());
        _memberChapterServiceRequest = new(() => MemberChapterServiceRequest.Create(Chapter.Id, MemberServiceRequest));
        _memberServiceRequest = new(() => MemberServiceRequest.Create(CurrentMemberId, ServiceRequest));
    }

    public Chapter Chapter => _chapter ?? throw new OdkNotFoundException();

    public Chapter? ChapterOrDefault => _chapter;

    public Member CurrentMember => _currentMember ?? throw new OdkNotAuthenticatedException();

    public Guid CurrentMemberId => _currentMemberId.Value;

    public Guid? CurrentMemberIdOrDefault => ServiceRequest.CurrentMemberIdOrDefault;
    
    public Member? CurrentMemberOrDefault => _currentMember;

    public MemberChapterServiceRequest MemberChapterServiceRequest => _memberChapterServiceRequest.Value;

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform => ServiceRequest.Platform;

    public ServiceRequest ServiceRequest => _serviceRequest!;

    public async Task<ChapterAdminMember> GetCurrentChapterAdminMember()
    {
        if (_currentChapterAdminMember != null)
        {
            return _currentChapterAdminMember;
        }

        try
        {
            _currentChapterAdminMember = await _unitOfWork.ChapterAdminMemberRepository
                .GetByMemberId(CurrentMemberId, Chapter.Id).Run();
            return _currentChapterAdminMember;
        }
        catch
        {
            var currentMember = CurrentMember;
            if (currentMember.SiteAdmin)
            {
                _currentChapterAdminMember = new ChapterAdminMember
                {
                    ChapterId = Chapter.Id,
                    MemberId = currentMember.Id,
                    Role = ChapterAdminRole.Owner
                };
                return _currentChapterAdminMember;
            }

            throw;
        }
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
        _loaded = false;
        _serviceRequest = null;
    }

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(IUnitOfWork unitOfWork, bool verbose)
    {
        var chapterName = ServiceRequest.HttpRequestContext.ChapterName();
        if (!string.IsNullOrEmpty(chapterName))
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by name: '{chapterName}'");
            }

            return unitOfWork.ChapterRepository.GetByName(chapterName);
        }

        var slug = ServiceRequest.HttpRequestContext.ChapterSlug();
        if (!string.IsNullOrEmpty(slug))
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by slug: '{slug}'");
            }

            return unitOfWork.ChapterRepository.GetBySlug(slug);
        }

        var chapterId = ServiceRequest.HttpRequestContext.ChapterId();
        if (chapterId != null)
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by id: '{chapterId}'");
            }

            return _unitOfWork.ChapterRepository.GetByIdOrDefault(chapterId.Value);
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
        if (_loaded)
        {
            return this;
        }

        _serviceRequest = serviceRequest;

        var (chapter, currentMember) = await _unitOfWork.RunAsync(
            x => GetChapterQuery(x, verbose),
            x => CurrentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(CurrentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>());

        _chapter = chapter;
        _currentMember = currentMember;
        _loaded = true;

        return this;
    }
}