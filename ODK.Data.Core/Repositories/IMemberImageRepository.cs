using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberImageRepository : IWriteRepository<MemberImage>
{
    IDeferredQuerySingleOrDefault<MemberImage> GetByMemberId(Guid memberId);
}
