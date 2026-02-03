using ODK.Core.Members;

namespace ODK.Core.Chapters;

public static class ChapterAdminMemberExtensions
{
    private static readonly Dictionary<ChapterAdminRole, int> _roleLevels = new()
    {
        { ChapterAdminRole.Owner, 1 },
        { ChapterAdminRole.Admin, 2 },
        { ChapterAdminRole.Organiser, 3 }
    };

    public static bool HasAccessTo(
        this ChapterAdminMember? adminMember,
        ChapterAdminRole role,
        Member currentMember)
    {
        if (currentMember.SiteAdmin)
        {
            return true;
        }

        if (adminMember == null)
        {
            return false;
        }

        return _roleLevels[adminMember.Role] <= _roleLevels[role];
    }
}
