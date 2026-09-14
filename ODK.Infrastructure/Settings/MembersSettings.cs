namespace ODK.Infrastructure.Settings;

public class MembersSettings
{
    public required int AvatarSize { get; init; }

    /// <summary>
    /// How long after an invite is emailed before it can be emailed again. The invite goes to somebody who
    /// has not asked us for anything, so this is the only thing between an impatient organiser and several
    /// copies of it.
    /// </summary>
    public required int InviteResendCooldownHours { get; init; }

    public required int SiteAdminSearchLimit { get; init; }
}
