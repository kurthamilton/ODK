using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberEmailPreferenceRepository : IWriteRepository<MemberEmailPreference>
{
    IDeferredQueryMultiple<MemberEmailPreference> GetByChapterId(Guid chapterId, 
        MemberEmailPreferenceType type);

    IDeferredQueryMultiple<MemberEmailPreference> GetByMemberId(Guid memberId);

    IDeferredQuerySingleOrDefault<MemberEmailPreference> GetByMemberId(Guid memberId, 
        MemberEmailPreferenceType type);
}
