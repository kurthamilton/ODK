using ODK.Core.Chapters;

namespace ODK.Services.Security;

public static class ChapterAdminMemberExtensions
{
    public static bool HasAccessTo(
        this ChapterAdminMember chapterAdminMember, ChapterAdminSecurable securable)
    {
        var role = securable.Role();
        return chapterAdminMember.HasAccessTo(role);
    }
}
