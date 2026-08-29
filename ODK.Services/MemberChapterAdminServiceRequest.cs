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
            CurrentMemberOrDefault = request.CurrentMember,
            Environment = request.Environment,
            HttpRequestContext = request.HttpRequestContext,
            Platform = request.Platform,
            Securable = securable
        };
    }
}