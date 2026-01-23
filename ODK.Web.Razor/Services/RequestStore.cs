using System.Threading.Tasks;
using ODK.Core;
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
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Models;
using Stripe;
using HttpRequestContextImpl = ODK.Web.Common.Services.HttpRequestContext;

namespace ODK.Web.Razor.Services;

public class RequestStore : IRequestStore
{
    private Chapter? _chapter;
    private Member? _currentMember;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoggingService _loggingService;
    private bool _loaded;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    private readonly Lazy<OdkComponentContext> _componentContext;
    private readonly Lazy<Guid> _currentMemberId;
    private readonly Lazy<Guid?> _currentMemberIdOrDefault;
    private readonly Lazy<IHttpRequestContext> _httpRequestContext;
    private readonly Lazy<MemberServiceRequest> _memberServiceRequest;
    private readonly Lazy<PlatformType> _platform;
    private readonly Lazy<ServiceRequest> _serviceRequest;

    public RequestStore(
        IHttpContextAccessor httpContextAccessor,
        IPlatformProvider platformProvider,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService)
    {
        _httpContextAccessor = httpContextAccessor;
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;

        _componentContext = new(() => new OdkComponentContext
        {
            CurrentMemberIdOrDefault = CurrentMemberIdOrDefault,
            HttpRequestContext = HttpRequestContext,
            Platform = Platform
        });
        _currentMemberId = new(
            () => CurrentMemberIdOrDefault ?? throw new OdkNotAuthorizedException());
        _currentMemberIdOrDefault = new(
            () => _httpContextAccessor.HttpContext?.User?.MemberIdOrDefault());
        _httpRequestContext = new(
            () => HttpRequestContextImpl.Create(_httpContextAccessor.HttpContext?.Request));
        _memberServiceRequest = new(() => MemberServiceRequest.Create(CurrentMemberId, ServiceRequest));
        _platform = new(() => _platformProvider.GetPlatform(HttpRequestContext.RequestUrl));
        _serviceRequest = new(() => new ServiceRequest
        {
            HttpRequestContext = HttpRequestContext,
            Platform = Platform
        });
    }

    public OdkComponentContext ComponentContext => _componentContext.Value;

    public Guid CurrentMemberId => _currentMemberId.Value;

    public Guid? CurrentMemberIdOrDefault => _currentMemberIdOrDefault.Value;

    public IHttpRequestContext HttpRequestContext => _httpRequestContext.Value;

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform => _platform.Value;

    public ServiceRequest ServiceRequest => _serviceRequest.Value;

    public async Task<Chapter> GetChapter() =>
        await GetChapterOrDefault(verbose: true) ?? throw new OdkNotFoundException("Chapter not found");

    public Task<Chapter?> GetChapterOrDefault() => GetChapterOrDefault(verbose: true);

    public async Task<Member> GetCurrentMember()
        => await GetCurrentMemberOrDefault(verbose: true) ?? throw new OdkNotAuthenticatedException();

    public Task<Member?> GetCurrentMemberOrDefault() => GetCurrentMemberOrDefault(verbose: true);

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(IUnitOfWork unitOfWork, bool verbose)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null && verbose)
        {
            _loggingService.Error("_httpContextAccessor.HttpContext not found");
        }

        OdkAssertions.Exists(httpContext, "_httpContextAccessor.HttpContext not found");

        var chapterName = httpContext.ChapterName();
        if (!string.IsNullOrEmpty(chapterName))
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by name: '{chapterName}'");
            }

            return unitOfWork.ChapterRepository.GetByName(chapterName);
        }

        var slug = httpContext.ChapterSlug();
        if (!string.IsNullOrEmpty(slug))
        {
            _loggingService.Info($"RequestStore: getting chapter by slug: '{slug}'");
            return unitOfWork.ChapterRepository.GetBySlug(slug);
        }

        var chapterId = httpContext.ChapterId();
        if (chapterId != null)
        {
            return _unitOfWork.ChapterRepository.GetByIdOrDefault(chapterId.Value);
        }

        return new DefaultDeferredQuerySingleOrDefault<Chapter>();
    }

    private async Task<Chapter?> GetChapterOrDefault(bool verbose)
    {
        await Load(verbose);
        return _chapter;
    }

    private async Task<Member?> GetCurrentMemberOrDefault(bool verbose)
    {
        await Load(verbose);
        return _currentMember;
    }

    private async Task Load(bool verbose)
    {
        if (_loaded)
        {
            return;
        }

        var (chapter, currentMember) = await _unitOfWork.RunAsync(
            x => GetChapterQuery(x, verbose),
            x => CurrentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(CurrentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>());

        _chapter = chapter;
        _currentMember = currentMember;
        _loaded = true;
    }
}