namespace ODK.Data.Core.SocialMedia;

public class InstagramImageMetadataDto
{
    public required string? Alt { get; init; }

    public required string? ExternalId { get; init; }

    public required Guid Id { get; init; }

    public required int VersionInt { get; init; }
}