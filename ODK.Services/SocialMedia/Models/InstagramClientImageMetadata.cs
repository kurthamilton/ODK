namespace ODK.Services.SocialMedia.Models;

public class InstagramClientImageMetadata
{
    public required string? Alt { get; init; }

    public required int? Height { get; init; }

    public required string ExternalId { get; init; }

    public required bool IsVideo { get; init; }

    public required string Url { get; init; }

    public required int? Width { get; init; }
}