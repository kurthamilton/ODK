namespace ODK.Services.SocialMedia;

public class SocialMediaServiceSettings
{
    public required string InstagramChannelUrlFormat { get; init; }

    public required int InstagramFetchWaitSeconds { get; init; }

    public required string InstagramPostUrlFormat { get; init; }

    public required string InstagramTagUrlFormat { get; init; }

    public required string WhatsAppUrlFormat { get; init; }
}