using Microsoft.EntityFrameworkCore;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.Core.SocialMedia;
using ODK.Data.EntityFramework.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramPostRepository : ReadWriteRepositoryBase<InstagramPost>, IInstagramPostRepository
{
    public InstagramPostRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<InstagramPostDto> GetDtosByChapterId(Guid chapterId, int pageSize)
        => GetDtoQuery()
            .Where(x => x.Post.ChapterId == chapterId)
            .OrderByDescending(x => x.Post.Date)
            .Take(pageSize)
            .DeferredMultiple();

    public IDeferredQueryMultiple<InstagramPostDto> GetDtosByExternalIds(IReadOnlyCollection<string> externalIds)
        => GetDtoQuery()
            .Where(x => externalIds.Contains(x.Post.ExternalId))
            .DeferredMultiple();

    private IQueryable<InstagramPostDto> GetDtoQuery() =>
        from post in Set()
        join image in Set<InstagramImage>() on post.Id equals image.InstagramPostId into images
        select new InstagramPostDto
        {
            Images = images
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new InstagramImageMetadataDto
                {
                    Alt = x.Alt,
                    ExternalId = x.ExternalId,
                    Id = x.Id,
                    VersionInt = x.VersionInt
                })
                .ToArray(),
            Post = post
        };
}