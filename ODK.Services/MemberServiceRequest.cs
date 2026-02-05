using ODK.Core.Members;

namespace ODK.Services;

public class MemberServiceRequest : ServiceRequest, IMemberServiceRequest
{
    public required Member CurrentMember { get; init; }

    public static MemberServiceRequest Create(
        Member currentMember,
        IServiceRequest request)
    {
        return new MemberServiceRequest
        {
            CurrentMember = currentMember,
            CurrentMemberOrDefault = currentMember,
            HttpRequestContext = request.HttpRequestContext,
            Platform = request.Platform
        };
    }
}