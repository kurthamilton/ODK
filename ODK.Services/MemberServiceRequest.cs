using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberServiceRequest : ServiceRequest
{
    public MemberServiceRequest(
        Guid currentMemberId,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
        : base(httpRequestContext, platform)
    {
        CurrentMemberId = currentMemberId;
    }

    public MemberServiceRequest(
        Guid currentMemberId,
        ServiceRequest request)
        : this(currentMemberId, request.HttpRequestContext, request.Platform)
    {
    }

    public Guid CurrentMemberId { get; }
}
