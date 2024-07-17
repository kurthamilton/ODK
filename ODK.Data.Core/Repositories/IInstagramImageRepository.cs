using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Core.SocialMedia;

public interface IInstagramImageRepository : IWriteRepository<InstagramImage>
{
    IDeferredQuerySingleOrDefault<InstagramImage> GetByPostId(Guid instagramPostId);
}
