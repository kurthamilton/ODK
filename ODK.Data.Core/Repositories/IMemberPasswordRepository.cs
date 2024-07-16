using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberPasswordRepository : IWriteRepository<MemberPassword>
{
    IDeferredQuerySingle<MemberPassword> GetByMemberId(Guid memberId);
}
