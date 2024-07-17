using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IMemberPropertyRepository : IReadWriteRepository<MemberProperty>
{
    IDeferredQueryMultiple<MemberProperty> GetByMemberId(Guid memberId);
}
