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

        // ChapterAdminRole.None is deliberately absent from _roleLevels: it grants nothing and
        // ranks against nothing, so an indexer lookup would throw rather than deny.
        return _roleLevels.TryGetValue(adminMember.Role, out var memberLevel)
            && _roleLevels.TryGetValue(role, out var requiredLevel)
            && memberLevel <= requiredLevel;
    }
}
