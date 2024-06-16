namespace ODK.Core.SocialMedia;

public interface IInstagramRepository
{
    Task AddImageAsync(InstagramImage image);

    Task AddPostAsync(InstagramPost post);

    Task<InstagramImage?> GetImageAsync(Guid instagramPostId);

    Task<DateTime?> GetLastPostDateAsync(Guid chapterId);

    Task<IReadOnlyCollection<InstagramPost>> GetPostsAsync(Guid chapterId, int pageSize);
}
