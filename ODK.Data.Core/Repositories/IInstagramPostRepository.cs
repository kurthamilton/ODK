using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Core.SocialMedia;

public interface IInstagramPostRepository : IReadWriteRepository<InstagramPost>
{    
    IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId, int pageSize);

    IDeferredQuerySingleOrDefault<InstagramPost> GetLastPost(Guid chapterId);
}
