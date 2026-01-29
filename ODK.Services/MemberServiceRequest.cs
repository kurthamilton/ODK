using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberServiceRequest : ServiceRequest
{
    public required Guid CurrentMemberId { get; init; }

    public static MemberServiceRequest Create(Guid currentMemberId, ServiceRequest request)
        => Create(currentMemberId, request.HttpRequestContext, request.Platform);

    public static MemberServiceRequest Create(
        Guid currentMemberId,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        return new MemberServiceRequest
        {
            CurrentMemberId = currentMemberId,
            CurrentMemberIdOrDefault = currentMemberId,
            HttpRequestContext = httpRequestContext,
            Platform = platform
        };
    }
}