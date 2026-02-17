using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;

public interface ISocialMediaService
{
    string GetInstagramChannelUrl(string username);

    string GetInstagramHashtagUrl(string hashtag);

    Task<InstagramImage> GetInstagramImage(Guid id);

    string GetInstagramPostUrl(string externalId);

    string GetWhatsAppLink(string groupId);

    Task ScrapeLatestInstagramPosts();

    Task<ServiceResult> ScrapeLatestInstagramPosts(Guid chapterId);
}