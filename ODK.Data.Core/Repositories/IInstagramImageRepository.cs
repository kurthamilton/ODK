using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Core.SocialMedia;

public interface IInstagramImageRepository : IWriteRepository<InstagramImage>
{
    IDeferredQuerySingle<InstagramImage> GetByPostId(Guid instagramPostId);
}
