using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.SocialMedia;

namespace ODK.Data.Core.Repositories;

public interface IInstagramPostRepository : IReadWriteRepository<InstagramPost>
{
    IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<InstagramPostDto> GetDtosByChapterId(Guid chapterId, int pageSize);

    IDeferredQueryMultiple<InstagramPostDto> GetDtosByExternalIds(IReadOnlyCollection<string> externalIds);
}