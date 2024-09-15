namespace ODK.Web.Common.Config.Settings;

public record OAuthGoogleSettings
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}
