using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberPreferencesRepository : IWriteRepository<MemberPreferences>
{
    IDeferredQuerySingleOrDefault<MemberPreferences> GetByMemberIdOrDefault(Guid memberId);

    IDeferredQueryMultiple<MemberPreferences> GetByMemberIds(IEnumerable<Guid> memberIds);

    void Upsert(MemberPreferences memberPreferences, Guid memberId);
}
