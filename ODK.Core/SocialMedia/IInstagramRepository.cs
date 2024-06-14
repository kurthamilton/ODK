namespace ODK.Core.SocialMedia;

public interface IInstagramRepository
{
    Task AddImage(InstagramImage image);

    Task AddPost(InstagramPost post);

    Task<InstagramImage> GetImage(Guid instagramPostId);

    Task<DateTime?> GetLastPostDate(Guid chapterId);

    Task<IReadOnlyCollection<InstagramPost>> GetPosts(Guid chapterId, int pageSize);
}
