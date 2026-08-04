using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberPreferencesRepository : WriteRepositoryBase<MemberPreferences>, IMemberPreferencesRepository
{
    public MemberPreferencesRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberPreferences> GetByMemberIdOrDefault(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<MemberPreferences> GetByMemberIds(IEnumerable<Guid> memberIds)
        => Set()
            .Where(x => memberIds.Contains(x.MemberId))
            .DeferredMultiple();

    public void Upsert(MemberPreferences memberPreferences, Guid memberId)
    {
        if (memberPreferences.MemberId == default)
        {
            memberPreferences.MemberId = memberId;
            Add(memberPreferences);
        }
        else
        {
            Update(memberPreferences);
        }
    }
}