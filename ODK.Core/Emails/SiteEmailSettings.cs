namespace ODK.Core.Emails;

/// <summary>
/// A platform's own email wording: what an email is addressed from, and the titles a group inherits where
/// it has set none of its own.
/// </summary>
public class SiteEmailSettings
{
    public required string AdminTitle { get; init; }

    public required string FromEmailAddress { get; init; }

    public required string MemberTitle { get; init; }
}
