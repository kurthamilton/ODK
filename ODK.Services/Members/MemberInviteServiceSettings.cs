namespace ODK.Services.Members;

public record MemberInviteServiceSettings
{
    public required int RetentionDays { get; init; }
}
