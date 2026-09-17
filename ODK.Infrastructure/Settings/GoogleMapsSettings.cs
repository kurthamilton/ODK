namespace ODK.Infrastructure.Settings;

public class GoogleMapsSettings
{
    /// <summary>
    /// Rendered into a script URL and a map iframe, so it is public by nature and wants an HTTP referrer
    /// restriction. Never use it for a call made from the server.
    /// </summary>
    public required string ClientApiKey { get; init; }
}
