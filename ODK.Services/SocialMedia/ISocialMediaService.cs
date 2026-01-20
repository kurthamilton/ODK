using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;

public interface ISocialMediaService
{
    Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid id);

    string GetInstagramChannelUrl(string username);

    string GetInstagramHashtagUrl(string hashtag);

    string GetInstagramPostUrl(string externalId);

    string GetWhatsAppLink(string groupId);

    Task ScrapeLatestInstagramPosts();

    Task<ServiceResult> ScrapeLatestInstagramPosts(Guid chapterId);
}