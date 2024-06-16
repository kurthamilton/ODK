using ODK.Core.SocialMedia;
using ODK.Data.Sql;

namespace ODK.Data.Repositories;

public class InstagramRepository : RepositoryBase, IInstagramRepository
{
    public InstagramRepository(SqlContext context) 
        : base(context)
    {
    }

    public async Task AddImageAsync(InstagramImage image)
    {
        await Context.Insert(image).ExecuteAsync();
    }

    public async Task AddPostAsync(InstagramPost post)
    {
        await Context.Insert(post).ExecuteAsync();
    }

    public async Task<InstagramImage?> GetImageAsync(Guid instagramPostId)
    {
        return await Context
            .Select<InstagramImage>()
            .Where(x => x.InstagramPostId).EqualTo(instagramPostId)
            .FirstOrDefaultAsync();
    }

    public async Task<DateTime?> GetLastPostDateAsync(Guid chapterId)
    {
        var mostRecent = await Context
            .Select<InstagramPost>()
            .OrderBy(x => x.Date, SqlSortDirection.Descending)
            .Page(1, 1)
            .FirstOrDefaultAsync();
        return mostRecent?.Date;
    }

    public async Task<IReadOnlyCollection<InstagramPost>> GetPostsAsync(Guid chapterId, int pageSize)
    {
        return await Context
            .Select<InstagramPost>()
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .OrderBy(x => x.Date, SqlSortDirection.Descending)
            .Page(1, pageSize)
            .ToArrayAsync();

    }
}
