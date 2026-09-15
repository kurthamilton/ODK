namespace ODK.Services.Members;

public record MemberInviteServiceSettings
{
    /// <summary>How long after an invite is emailed before it can be emailed again.</summary>
    public required int ResendCooldownHours { get; init; }

    public required int RetentionDays { get; init; }
}
