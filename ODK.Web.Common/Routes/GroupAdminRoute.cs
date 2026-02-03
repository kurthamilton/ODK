using Microsoft.AspNetCore.Server.IISIntegration;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Security;

namespace ODK.Web.Common.Routes;

public class GroupAdminRoute
{
    public static readonly GroupAdminRoute Default = new() { IsDefault = true, Path = "/", Securable = ChapterAdminSecurable.None };
    
    public bool IsDefault { get; private set; }

    public GroupAdminRoute? Parent { get; private set; }

    public required string Path { get; init; }

    public PlatformType? Platform { get; init; }

    public required ChapterAdminSecurable Securable { get; init; }        

    public GroupAdminRoute Child(
        string subPath, 
        ChapterAdminSecurable? securable = null,
        PlatformType? platform = null)
    {
        if (IsDefault)
        {
            return Default;
        }

        return new GroupAdminRoute
        {
            Parent = this,
            Path = Path + subPath,
            Platform = platform ?? Platform,
            Securable = securable ?? Securable
        };
    }

    public GroupAdminRoute? GetPermitted(ChapterAdminMember? chapterAdminMember, Member currentMember)
        => chapterAdminMember.HasAccessTo(Securable, currentMember)
            ? this
            : Parent?.GetPermitted(chapterAdminMember, currentMember);
}
