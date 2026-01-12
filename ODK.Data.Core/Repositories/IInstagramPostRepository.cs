using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IInstagramPostRepository : IReadWriteRepository<InstagramPost>
{
    IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId, int pageSize);

    IDeferredQuerySingleOrDefault<InstagramPost> GetLastPost(Guid chapterId);
}
