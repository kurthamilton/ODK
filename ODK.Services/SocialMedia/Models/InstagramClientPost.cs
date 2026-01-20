namespace ODK.Services.SocialMedia.Models;

public class InstagramClientPost
{
    public required string? Caption { get; init; }

    public required DateTime Date { get; init; }

    public required string ExternalId { get; init; }

    public required IReadOnlyCollection<InstagramClientImageMetadata> Images { get; init; }
}