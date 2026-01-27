using System.Collections.Generic;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Security;

namespace ODK.Web.Common.Components;

public class ChapterAdminMenuItemGroup : List<MenuItem>
{
    private readonly ChapterAdminMember _adminMember;
    private readonly PlatformType _platform;

    public ChapterAdminMenuItemGroup(ChapterAdminMember adminMember, PlatformType platform)
    {
        _adminMember = adminMember;
        _platform = platform;
    }

    public ChapterAdminMenuItemGroup Add(
        ChapterAdminSecurable securable, MenuItem menuItem, PlatformType? platform = null)
    {
        if (!_adminMember.HasAccessTo(securable.Role()) ||
            (platform != null && platform != _platform))
        {
            return this;
        }

        Add(menuItem);

        return this;
    }
}
