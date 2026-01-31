using System.Collections.Generic;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Security;

namespace ODK.Web.Common.Components;

public class ChapterAdminMenuItemGroup : List<MenuItem>
{
    private readonly ChapterAdminMember? _adminMember;
    private readonly Member _currentMember;
    private readonly PlatformType _platform;

    public ChapterAdminMenuItemGroup(
        Member currentMember, ChapterAdminMember? adminMember, PlatformType platform)
    {
        _adminMember = adminMember;
        _currentMember = currentMember;
        _platform = platform;
    }

    public ChapterAdminMenuItemGroup Add(
        ChapterAdminSecurable securable, MenuItem menuItem, PlatformType? platform = null)
    {
        if (!_adminMember.HasAccessTo(securable, _currentMember) ||
            (platform != null && platform != _platform))
        {
            return this;
        }

        Add(menuItem);

        return this;
    }
}
