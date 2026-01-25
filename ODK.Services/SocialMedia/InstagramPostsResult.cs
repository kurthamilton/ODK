using ODK.Services.SocialMedia.Models;

namespace ODK.Services.SocialMedia;

public class InstagramPostsResult : ServiceResult
{
    public InstagramPostsResult(bool success, string? message = null)
        : base(success, message)
    {
    }

    public InstagramPostsResult(IReadOnlyCollection<InstagramClientPost> posts)
        : base(true)
    {
        Posts = posts;
    }

    public IReadOnlyCollection<InstagramClientPost>? Posts { get; }

    public required string? Response { get; init; }
}