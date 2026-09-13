namespace ODK.Services.Members;

public record MemberAdminServiceSettings
{
    public required int InviteRetentionDays { get; init; }

    public required int MemberAvatarSize { get; init; }

    /// <summary>
    /// How many members a site admin's search returns before it reports itself as truncated.
    /// </summary>
    public required int SiteAdminMemberSearchLimit { get; init; }
}
