using ODK.Core.SocialMedia;

namespace ODK.Data.Core.SocialMedia;

public class InstagramPostDto
{
    public required IReadOnlyCollection<InstagramImageMetadataDto> Images { get; init; }

    public required InstagramPost Post { get; init; }
}