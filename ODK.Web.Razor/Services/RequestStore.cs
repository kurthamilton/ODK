using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Models;
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
    private readonly RequestStoreSettings _settings;
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
        ILoggingService loggingService,
        RequestStoreSettings settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _settings = settings;
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

    public async Task<Chapter> GetChapter()
    {
        var chapter = await GetChapterOrDefault(verbose: false);
        if (chapter == null)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var path = request?.Path.Value.EnsureTrailing("/") ?? "/";

            var reload = true;

            if (_settings.IgnoreNotFoundPaths.Any(path.StartsWith) ||
                _settings.IgnoreNotFoundPathPatterns.Any(x => Regex.IsMatch(path, x)))
            {
                // don't reload with verbose logging if we don't want to log for the current path
                reload = false;
            }

            // re-run chapter load with verbose logging
            var message = "Chapter not found when one was expected";
            if (reload)
            {
                Reset();

                var userAgent = request?.Headers.UserAgent.ToString() ?? "";
                if (_settings.WarningNotFoundUserAgents.Any(x => userAgent.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    await _loggingService.Warn(message);
                }
                else
                {
                    await _loggingService.Error(message);
                }

                chapter = await GetChapterOrDefault(verbose: true);
            }

            throw new OdkNotFoundException(message);
        }

        return chapter;
    }

    public Task<Chapter?> GetChapterOrDefault() => GetChapterOrDefault(verbose: false);

    public async Task<Member> GetCurrentMember()
        => await GetCurrentMemberOrDefault(verbose: false) ?? throw new OdkNotAuthenticatedException();

    public Task<Member?> GetCurrentMemberOrDefault() => GetCurrentMemberOrDefault(verbose: false);

    public void Reset()
    {
        _chapter = null;
        _currentMember = null;
        _loaded = false;
    }

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
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by slug: '{slug}'");
            }

            return unitOfWork.ChapterRepository.GetBySlug(slug);
        }

        var chapterId = httpContext.ChapterId();
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
                { "Url", httpContext.Request.GetDisplayUrl() }
            };

            foreach (var routeValue in httpContext.Request.RouteValues)
            {
                properties.Add(routeValue.Key, routeValue.Value?.ToString());
            }

            _loggingService.Warn(message, properties);
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