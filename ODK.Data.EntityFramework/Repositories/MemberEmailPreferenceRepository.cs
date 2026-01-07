using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberEmailPreferenceRepository : WriteRepositoryBase<MemberEmailPreference>, IMemberEmailPreferenceRepository
{
    public MemberEmailPreferenceRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberEmailPreference> GetByChapterId(Guid chapterId, MemberEmailPreferenceType type)
    {
        var query =
            from preference in Set()
            from member in Set<Member>().InChapter(chapterId)
            where preference.MemberId == member.Id
                && preference.Type == type
            select preference;

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<MemberEmailPreference> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberEmailPreference> GetByMemberId(Guid memberId,
        MemberEmailPreferenceType type) => Set()
        .Where(x => x.MemberId == memberId && x.Type == type)
        .DeferredSingleOrDefault();
}
