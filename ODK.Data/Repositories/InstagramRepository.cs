using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.SocialMedia;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class InstagramRepository : RepositoryBase, IInstagramRepository
    {
        public InstagramRepository(SqlContext context) 
            : base(context)
        {
        }

        public async Task AddImage(InstagramImage image)
        {
            await Context.Insert(image).ExecuteAsync();
        }

        public async Task AddPost(InstagramPost post)
        {
            await Context.Insert(post).ExecuteAsync();
        }

        public async Task<InstagramImage> GetImage(Guid instagramPostId)
        {
            return await Context
                .Select<InstagramImage>()
                .Where(x => x.InstagramPostId).EqualTo(instagramPostId)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> GetLastPostDate(Guid chapterId)
        {
            InstagramPost mostRecent = await Context
                .Select<InstagramPost>()
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Page(1, 1)
                .FirstOrDefaultAsync();
            return mostRecent?.Date;
        }

        public async Task<IReadOnlyCollection<InstagramPost>> GetPosts(Guid chapterId, int pageSize)
        {
            return await Context
                .Select<InstagramPost>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .OrderBy(x => x.Date, SqlSortDirection.Descending)
                .Page(1, pageSize)
                .ToArrayAsync();

        }
    }
}
