using Microsoft.EntityFrameworkCore;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.Core.SocialMedia;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramPostRepository : ReadWriteRepositoryBase<InstagramPost>, IInstagramPostRepository
{
    public InstagramPostRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId) 
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<InstagramPostDto> GetDtosByChapterId(Guid chapterId, int pageSize)
    {
        var query =
            from post in Set()
            join image in Set<InstagramImage>() on post.Id equals image.InstagramPostId into images
            where post.ChapterId == chapterId
            select new InstagramPostDto
            {
                ImageIds = images
                    .Select(x => x.Id)
                    .ToArray(),
                Post = post
            };

        return query
            .OrderByDescending(x => x.Post.Date)
            .Take(pageSize)
            .DeferredMultiple();
    }

    public IDeferredQueryMultiple<string> GetExternalIdsByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .Select(x => x.ExternalId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<InstagramPost> GetLastPost(Guid chapterId) 
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .OrderByDescending(x => x.Date)
            .DeferredSingleOrDefault();
}
