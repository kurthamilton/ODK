using ODK.Services.SocialMedia.Models;

namespace ODK.Services.SocialMedia;

public interface IInstagramClient
{
    Task<IReadOnlyCollection<InstagramClientPost>> FetchPosts(string username, DateTime? afterUtc);
}