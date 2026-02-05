using ODK.Services.Security;

namespace ODK.Services;

public class MemberChapterAdminServiceRequest : MemberChapterServiceRequest, IMemberChapterAdminServiceRequest
{
    public required ChapterAdminSecurable Securable { get; init; }

    public static MemberChapterAdminServiceRequest Create(
        ChapterAdminSecurable securable, IMemberChapterServiceRequest request)
    {
        return new MemberChapterAdminServiceRequest
        {
            Chapter = request.Chapter,
            CurrentMember = request.CurrentMember,
            CurrentMemberIdOrDefault = request.CurrentMember.Id,
            HttpRequestContext = request.HttpRequestContext,
            Platform = request.Platform,
            Securable = securable
        };
    }
}