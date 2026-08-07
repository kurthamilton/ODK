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
    {
        // Default is the "route does not exist on this platform" sentinel — it is never a
        // destination, and its None securable has no role to compare against.
        if (IsDefault)
        {
            return null;
        }

        return chapterAdminMember.HasAccessTo(Securable, currentMember)
            ? this
            : Parent?.GetPermitted(chapterAdminMember, currentMember);
    }

    public bool IsPermitted(ChapterAdminMember? chapterAdminMember, Member currentMember, PlatformType platform)
        => !IsDefault
            && (Platform == null || Platform == platform)
            && chapterAdminMember.HasAccessTo(Securable, currentMember);

    /// <summary>
    /// Renders the path, so a route written straight into markup as <c>@route</c> emits the URL rather
    /// than the type name. Every string-typed use is a compile error without <see cref="Path"/> — this
    /// closes the one context where the compiler cannot help.
    /// </summary>
    public override string ToString() => Path;
}
