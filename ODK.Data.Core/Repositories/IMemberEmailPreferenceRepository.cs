using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberEmailPreferenceRepository : IWriteRepository<MemberEmailPreference>
{
    public IDeferredQueryMultiple<MemberEmailPreference> GetByChapterId(Guid chapterId, MemberEmailPreferenceType type);

    public IDeferredQueryMultiple<MemberEmailPreference> GetByMemberId(Guid memberId);
}
