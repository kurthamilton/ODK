using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;

namespace ODK.Data.Core.Repositories;

public interface IMemberAvatarRepository : IWriteRepository<MemberAvatar>
{
    IDeferredQuerySingleOrDefault<MemberAvatar> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<MemberAvatarVersionDto> GetVersionDtoByMemberId(Guid memberId);
}