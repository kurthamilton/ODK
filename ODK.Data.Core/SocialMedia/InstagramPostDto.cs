using ODK.Core.SocialMedia;

namespace ODK.Data.Core.SocialMedia;

public class InstagramPostDto
{
    public required IReadOnlyCollection<Guid> ImageIds { get; init; }

    public required InstagramPost Post { get; init; }
}
