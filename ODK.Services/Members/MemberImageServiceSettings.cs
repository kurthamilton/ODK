namespace ODK.Services.Members;

public record MemberImageServiceSettings
{
    public required int MaxImageSize { get; init; }

    public required int MemberAvatarSize { get; init; }
}
