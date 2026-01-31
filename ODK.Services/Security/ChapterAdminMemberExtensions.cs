using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Security;

public static class ChapterAdminMemberExtensions
{
    public static bool HasAccessTo(
        this ChapterAdminMember? chapterAdminMember, 
        ChapterAdminSecurable securable, 
        Member currentMember)
    {
        if (currentMember.SiteAdmin)
        {
            return true;
        }

        if (chapterAdminMember == null)
        {
            return false;
        }

        var role = securable.Role();
        return chapterAdminMember.HasAccessTo(role);
    }
}
