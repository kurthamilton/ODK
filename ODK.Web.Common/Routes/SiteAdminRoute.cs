namespace ODK.Web.Common.Routes;

/// <summary>
/// A site admin (superadmin) route. Site admin access is binary — a member either has
/// <see cref="ODK.Core.Members.Member.SiteAdmin"/> or does not — so unlike
/// <see cref="GroupAdminRoute"/> there is no securable to carry and nothing to filter on.
/// </summary>
/// <remarks>
/// Deliberately has no implicit string conversion: using a route where a string is expected should be
/// a compile error that <see cref="Path"/> resolves, not a silent success. <see cref="ToString"/>
/// covers the one context the compiler cannot police — a route written into markup as <c>@route</c>.
/// </remarks>
public class SiteAdminRoute
{
    public SiteAdminRoute(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public SiteAdminRoute Child(string subPath) => new(Path + subPath);

    public override string ToString() => Path;
}
