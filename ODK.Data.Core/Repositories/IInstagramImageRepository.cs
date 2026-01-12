using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IInstagramImageRepository : IWriteRepository<InstagramImage>
{
    IDeferredQuerySingle<InstagramImage> GetByPostId(Guid instagramPostId);
}
