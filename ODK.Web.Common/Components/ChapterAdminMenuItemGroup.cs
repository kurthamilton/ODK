using System.Collections.Generic;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Security;
using ODK.Web.Common.Routes;

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
        GroupAdminRoute route, string text)
    {
        if (!_adminMember.HasAccessTo(route.Securable, _currentMember))
        {
            return this;
        }

        if (route.Platform != null && route.Platform != _platform)
        {
            return this;
        }

        if (route.IsDefault)
        {
            return this;
        }

        Add(new MenuItem
        {
            Link = route.Path,
            Text = text,
        });

        return this;
    }
}
