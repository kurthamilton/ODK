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
    /// The rendered email, which only the layout template uses. Set by <see cref="EmailService"/> once
    /// every other parameter has been resolved, since it is the result of interpolating them.
    /// </summary>
    public const string BodyName = "body";

    /// <summary>
    /// Marks a parameter whose value is HTML and must be interpolated without encoding. A template
    /// refers to the parameter by its plain name; only the supplied key carries the prefix.
    /// </summary>
    public const string HtmlPrefix = "html:";

    /// <summary>
    /// The wording an email refers to its group by, and the only title a template names. Its value follows
    /// the email's own <see cref="Core.Emails.Email.RecipientType"/> - an email written for admins takes the
    /// admin title, one written for members the member title - so a template author has no audience to
    /// choose between and cannot choose wrongly. Each title is the group's where it has set one and the
    /// site's otherwise.
    /// </summary>
    /// <remarks>
    /// Has no property here, and cannot have one: the title is itself a template over the other parameters,
    /// so <see cref="EmailService"/> resolves it only once they have all been merged. A row in
    /// <see cref="Values"/> would put the unresolved template into the email instead of its value. Named
    /// here so <see cref="Names"/> stays complete.
    /// </remarks>
    public const string TitleName = "title";

    private const string GroupPrefix = "group.";

    private const string ThemePrefix = "theme.";

    /* One table for both the values and the names, so the list offered to an admin cannot drift from
       the list the app actually supplies. Adding a property means adding a row here. */
    private static readonly (string Name, Func<EmailParameters, string?> Value)[] Values =
    [
        ("group.fullname", x => x.GroupFullName),
        ("group.name", x => x.GroupName),
        ("group.url", x => x.GroupUrl),
        ("platform.url", x => x.PlatformUrl),
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

    /// <summary>
    /// The colours the layout styles itself with. Supplied to every email like the rest, but only worth
    /// offering on the layout, which is the only template holding the markup they apply to.
    /// </summary>
    public static IReadOnlyCollection<string> ThemeNames { get; } = Names
        .Where(x => x.StartsWith(ThemePrefix, StringComparison.Ordinal))
        .ToArray();

    public string? GroupUrl { get; set; }

    public string? GroupFullName { get; set; }

    public string? GroupName { get; set; }

    public string? PlatformUrl { get; set; }

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
