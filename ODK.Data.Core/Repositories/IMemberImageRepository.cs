using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.Repositories;

public interface IMemberImageRepository : IWriteRepository<MemberImage>
{
    IDeferredQuerySingleOrDefault<MemberImage> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<MemberImageVersionDto> GetVersionDtoByMemberId(Guid memberId);
}