namespace ODK.Services.Members;

public record MemberAdminServiceSettings
{
    public required int InviteRetentionDays { get; init; }

    public required int MemberAvatarSize { get; init; }
}
