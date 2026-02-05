using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services;

public class MemberChapterServiceRequest : MemberServiceRequest, IMemberChapterServiceRequest
{
    public MemberChapterServiceRequest()
    {
    }

    public required Chapter Chapter { get; init; }

    public static MemberChapterServiceRequest Create(Chapter chapter, IMemberServiceRequest request)
        => Create(chapter, request.CurrentMember, request);

    public static MemberChapterServiceRequest Create(
        Chapter chapter,
        Member currentMember,
        IServiceRequest request)
    {
        return new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            CurrentMemberOrDefault = currentMember,
            HttpRequestContext = request.HttpRequestContext,
            Platform = request.Platform
        };
    }
}