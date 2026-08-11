namespace ODK.Services.Emails;

/// <summary>
/// The parameters every email gets, whatever it is about: the group it belongs to, the platform, and
/// the theme colours. <see cref="EmailService"/> fills these in from the request, then merges in
/// whatever the caller supplies on top.
/// </summary>
/// <remarks>
/// Against the usual convention nothing here is required and every property has a setter: the values
/// are resolved one at a time from different sources, and a property left null is simply omitted, so
/// a template referencing it renders the token verbatim rather than an empty string.
/// </remarks>
public sealed class EmailParameters : IEmailParameters
{
    /// <summary>
    /// Has no property here: the title is interpolated from the other parameters, so it can only be
    /// resolved after they have all been merged. Named here so <see cref="Names"/> stays complete.
    /// </summary>
    public const string TitleName = "title";

    private const string GroupPrefix = "group.";

    /* One table for both the values and the names, so the list offered to an admin cannot drift from
       the list the app actually supplies. Adding a property means adding a row here. */
    private static readonly (string Name, Func<EmailParameters, string?> Value)[] Values =
    [
        ("group.baseurl", x => x.GroupBaseUrl),
        ("group.fullname", x => x.GroupFullName),
        ("group.name", x => x.GroupName),
        ("platform.baseurl", x => x.PlatformBaseUrl),
        ("theme.body.background", x => x.ThemeBodyBackground),
        ("theme.body.color", x => x.ThemeBodyColor),
        ("theme.header.background", x => x.ThemeHeaderBackground),
        ("theme.header.color", x => x.ThemeHeaderColor)
    ];

    /// <summary>
    /// Every parameter an email template can rely on, whichever email it is.
    /// </summary>
    public static IReadOnlyCollection<string> Names { get; } = Values
        .Select(x => x.Name)
        .Append(TitleName)
        .Order(StringComparer.Ordinal)
        .ToArray();

    /// <summary>
    /// The subset offered to a group admin. The rest describe the platform and the theme, which are the
    /// site's to set rather than a group's - a group template referencing them would still resolve, but
    /// offering them invites edits to something the group does not control.
    /// </summary>
    public static IReadOnlyCollection<string> GroupNames { get; } = Names
        .Where(x => x.StartsWith(GroupPrefix, StringComparison.Ordinal) || x == TitleName)
        .ToArray();

    public string? GroupBaseUrl { get; set; }

    public string? GroupFullName { get; set; }

    public string? GroupName { get; set; }

    public string? PlatformBaseUrl { get; set; }

    public string? ThemeBodyBackground { get; set; }

    public string? ThemeBodyColor { get; set; }

    public string? ThemeHeaderBackground { get; set; }

    public string? ThemeHeaderColor { get; set; }

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var values = new Dictionary<string, string>(EmailParameterComparer.Default);

        foreach (var (name, value) in Values)
        {
            var resolved = value(this);
            if (resolved != null)
            {
                values[name] = resolved;
            }
        }

        return values;
    }
}
