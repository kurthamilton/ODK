namespace ODK.Services.Integrations.Instagram.Models;

public class InstagramImageResponse
{
    public required int? Height { get; init; }

    public required bool IsVideo { get; init; }

    public required string Shortcode { get; init; }

    public required string Url { get; init; }

    public required int? Width { get; init; }
}