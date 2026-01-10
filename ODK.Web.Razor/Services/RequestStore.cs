using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models;

namespace ODK.Web.Razor.Services;

public class RequestStore : IRequestStore
{
    private Chapter? _chapter;
    private Member? _currentMember;
    private readonly IHttpContextAccessor _httpContextAccessor;
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
        IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
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
            () => new HttpRequestContext(_httpContextAccessor.HttpContext?.Request));
        _memberServiceRequest = new(() => new MemberServiceRequest(CurrentMemberId, ServiceRequest));
        _platform = new(() => _platformProvider.GetPlatform(HttpRequestContext.RequestUrl));
        _serviceRequest = new(() => new ServiceRequest(HttpRequestContext, Platform));
    }

    public OdkComponentContext ComponentContext => _componentContext.Value;

    public Guid CurrentMemberId => _currentMemberId.Value;

    public Guid? CurrentMemberIdOrDefault => _currentMemberIdOrDefault.Value;

    public IHttpRequestContext HttpRequestContext => _httpRequestContext.Value;

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform => _platform.Value;

    public ServiceRequest ServiceRequest => _serviceRequest.Value;

    public async Task<Chapter> GetChapter()
    {
        var chapter = await GetChapterOrDefault();
        OdkAssertions.Exists(chapter);
        return chapter;
    }

    public async Task<Chapter?> GetChapterOrDefault()
    {
        await Load();
        return _chapter;
    }

    public async Task<Member> GetCurrentMember()
        => await GetCurrentMemberOrDefault() ?? throw new OdkNotAuthenticatedException();

    public async Task<Member?> GetCurrentMemberOrDefault()
    {
        await Load();
        return _currentMember;
    }

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(IUnitOfWork unitOfWork)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        OdkAssertions.Exists(httpContext);

        var chapterName = httpContext.ChapterName();
        if (!string.IsNullOrEmpty(chapterName))
        {
            return unitOfWork.ChapterRepository.GetByName(chapterName);
        }

        var slug = httpContext.ChapterSlug();
        if (!string.IsNullOrEmpty(slug))
        {
            return unitOfWork.ChapterRepository.GetBySlug(slug);
        }

        var chapterId = httpContext.ChapterId();
        if (chapterId != null)
        {
            return _unitOfWork.ChapterRepository.GetByIdOrDefault(chapterId.Value);
        }

        return new DefaultDeferredQuerySingleOrDefault<Chapter>();
    }

    private async Task Load()
    {
        if (_loaded)
        {
            return;
        }

        var (chapter, currentMember) = await _unitOfWork.RunAsync(
            x => GetChapterQuery(x),
            x => CurrentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(CurrentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>());

        _chapter = chapter;
        _currentMember = currentMember;
        _loaded = true;
    }
}