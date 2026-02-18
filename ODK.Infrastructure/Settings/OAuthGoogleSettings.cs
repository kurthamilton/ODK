namespace ODK.Infrastructure.Settings;

public record OAuthGoogleSettings
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}
