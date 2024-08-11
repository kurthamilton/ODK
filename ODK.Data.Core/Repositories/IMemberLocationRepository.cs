using ODK.Core.Members;

namespace ODK.Data.Core.Repositories;

public interface IMemberLocationRepository : IWriteRepository<MemberLocation>
{
    Task<MemberLocation> GetByMemberId(Guid memberId);
}
