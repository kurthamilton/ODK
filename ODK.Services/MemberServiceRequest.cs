using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberServiceRequest : ServiceRequest
{
    public required Member CurrentMember { get; init; }

    public static MemberServiceRequest Create(Member currentMember, ServiceRequest request)
        => Create(currentMember, request.HttpRequestContext, request.Platform);

    public static MemberServiceRequest Create(
        Member currentMember,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        return new MemberServiceRequest
        {
            CurrentMember = currentMember,
            CurrentMemberIdOrDefault = currentMember.Id,
            HttpRequestContext = httpRequestContext,
            Platform = platform
        };
    }
}