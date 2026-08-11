namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells group admins that someone has joined.
/// </summary>
public sealed class NewMemberAdminParameters : EmailTypeParameters
{
    private const string AdminUrlName = "admin.urls.member";

    /// <summary>
    /// Supplied under the HTML prefix, which is how EmailService knows to interpolate it without
    /// encoding. A template refers to it by the offered name, without the prefix.
    /// </summary>
    private const string PropertiesName = "member.properties";

    public static IReadOnlyCollection<string> Names { get; } = [AdminUrlName, PropertiesName];

    public string? AdminUrl { get; set; }

    public string? Properties { get; set; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, AdminUrlName, AdminUrl);
        Add(values, EmailParameters.HtmlPrefix + PropertiesName, Properties);
    }
}
