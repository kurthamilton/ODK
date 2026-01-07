using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;

public interface IInstagramService
{
    Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid instagramPostId);

    Task ScrapeLatestInstagramPosts();

    Task ScrapeLatestInstagramPosts(Guid chapterId);
}
