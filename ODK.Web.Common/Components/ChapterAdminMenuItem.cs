using ODK.Core.Chapters;
using ODK.Services.Security;

namespace ODK.Web.Common.Components;

public class ChapterAdminMenuItem : MenuItem
{
    private readonly bool _hasAccess;

    public ChapterAdminMenuItem(ChapterAdminMember adminMember, ChapterAdminSecurable securable)
    {
        _hasAccess = adminMember.HasAccessTo(securable);
    }
}