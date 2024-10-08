﻿using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;

public interface IInstagramService
{
    Task<IReadOnlyCollection<SocialMediaImage>> FetchInstagramImages(Guid chapterId);

    Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid instagramPostId);

    Task<InstagramPostsDto> GetInstagramPosts(Guid chapterId, int pageSize);

    Task ScrapeLatestInstagramPosts();

    Task ScrapeLatestInstagramPosts(Guid chapterId);
}
