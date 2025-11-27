using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models;

namespace ODK.Web.Razor.Services;

public class RequestStore : IRequestStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPlatformProvider _platformProvider;

    private readonly Lazy<OdkComponentContext> _componentContext;
    private readonly Lazy<Guid> _currentMemberId;
    private readonly Lazy<Guid?> _currentMemberIdOrDefault;
    private readonly Lazy<IHttpRequestContext> _httpRequestContext;
    private readonly Lazy<MemberServiceRequest> _memberServiceRequest;
    private readonly Lazy<PlatformType> _platform;
    private readonly Lazy<ServiceRequest> _serviceRequest;

    public RequestStore(
        IHttpContextAccessor httpContextAccessor,
        IPlatformProvider platformProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _platformProvider = platformProvider;

        _componentContext = new(() => new OdkComponentContext
        {
            CurrentMemberId = CurrentMemberIdOrDefault,
            HttpRequestContext = HttpRequestContext,
            Platform = Platform
        });
        _currentMemberId = new(
            () => _httpContextAccessor.HttpContext?.User?.MemberId() ?? throw new OdkNotAuthorizedException());
        _currentMemberIdOrDefault = new(
            () => _httpContextAccessor.HttpContext?.User?.MemberIdOrDefault());
        _httpRequestContext = new(
            () => new HttpRequestContext(_httpContextAccessor.HttpContext?.Request));
        _memberServiceRequest = new(() => new MemberServiceRequest(CurrentMemberId, ServiceRequest));
        _platform = new(() => _platformProvider.GetPlatform(HttpRequestContext.RequestUrl));
        _serviceRequest = new(() => new ServiceRequest(HttpRequestContext, Platform));
    }

    public Chapter? Chapter => throw new NotImplementedException();

    public OdkComponentContext ComponentContext => _componentContext.Value;

    public Guid CurrentMemberId => _currentMemberId.Value;

    public Guid? CurrentMemberIdOrDefault => _currentMemberIdOrDefault.Value;

    public IHttpRequestContext HttpRequestContext => _httpRequestContext.Value;

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform => _platform.Value;

    public ServiceRequest ServiceRequest => _serviceRequest.Value;
}
