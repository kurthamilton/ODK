using ODK.Services.SocialMedia.Models;

namespace ODK.Services.SocialMedia;

public interface IInstagramClient
{
    Task<InstagramClientImage> FetchImage(InstagramClientImageMetadata metadata);

    Task<IReadOnlyCollection<InstagramClientPost>> FetchPosts(
        string username, IReadOnlyCollection<string> excludeIds);
}