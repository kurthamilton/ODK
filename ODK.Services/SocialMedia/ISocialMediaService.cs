using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;

public interface ISocialMediaService
{
    Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid instagramPostId);

    string GetWhatsAppLink(string groupId);

    Task ScrapeLatestInstagramPosts();

    Task<ServiceResult> ScrapeLatestInstagramPosts(Guid chapterId);
}