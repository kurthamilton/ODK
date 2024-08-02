namespace ODK.Services.Members;

public record MemberServiceSettings
{
    public required string ActivateAccountUrl { get; init; }

    public required string ConfirmEmailAddressUpdateUrl { get; init; }

    public required int MaxImageSize { get; init; }

    public required int MemberAvatarSize { get; init; }

    public required bool UseAvatars { get; init; }
}
