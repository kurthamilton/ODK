namespace ODK.Infrastructure.Settings;

public record GoogleOAuthSettings
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}
