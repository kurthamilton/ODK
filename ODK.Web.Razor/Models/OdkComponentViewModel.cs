using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Models;

public abstract class OdkComponentViewModel
{
    private readonly Lazy<MemberServiceRequest> _memberServiceRequest;
    private readonly Lazy<ServiceRequest> _serviceRequest;

    protected OdkComponentViewModel(OdkComponentContext context)
    {
        Context = context;
        HttpRequestContext = context.HttpRequestContext;
        Platform = context.Platform;

        _memberServiceRequest =
            new(() => MemberServiceRequest.Create(
                Context.CurrentMemberIdOrDefault ?? throw new OdkNotAuthorizedException(),
                ServiceRequest));
        _serviceRequest = new(() => new ServiceRequest
        {
            HttpRequestContext = HttpRequestContext,
            Platform = Platform
        });
    }

    public OdkComponentContext Context { get; }

    public IHttpRequestContext HttpRequestContext { get; }

    public MemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform { get; }

    public ServiceRequest ServiceRequest => _serviceRequest.Value;

    public MemberChapterServiceRequest CreateMemberChapterServiceRequest(Guid chapterId)
        => MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);
}