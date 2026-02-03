using ODK.Core.Members;

namespace ODK.Core.Chapters;

public static class ChapterAdminRoleExtensions
{
    private static IReadOnlyDictionary<ChapterAdminRole, int> _roleLevels = new Dictionary<ChapterAdminRole, int>
    {
        { ChapterAdminRole.Owner, 1 },
        { ChapterAdminRole.Admin, 2 },
        { ChapterAdminRole.Organiser, 3 }
    };

    public static bool HasAccessTo(this ChapterAdminRole? currentAdminRole, ChapterAdminRole targetRole, Member currentMember)
    {
        if (currentMember.SiteAdmin)
        {
            return true;
        }

        if (currentAdminRole == null)
        {
            return false;
        }

        return _roleLevels[currentAdminRole.Value] <= _roleLevels[targetRole];
    }

    public static bool HasAccessTo(this ChapterAdminRole currentAdminRole, ChapterAdminRole targetRole, Member currentMember)
        => HasAccessTo((ChapterAdminRole?)currentAdminRole, targetRole, currentMember);
}