namespace ODK.Services.Integrations.Instagram.Models;

public class InstagramPostResponse
{
    public required string? Caption { get; init; }

    public required DateTime DateUtc { get; init; }

    public required string Shortcode { get; init; }
}