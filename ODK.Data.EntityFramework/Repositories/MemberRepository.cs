using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberRepository : ReadWriteRepositoryBase<Member>, IMemberRepository
{
    public MemberRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress) => Set()
        .Where(x => x.EmailAddress == emailAddress)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Member> GetAllByChapterId(Guid chapterId) => Set()
        .InChapter(chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId) => Set()
        .Current(Set<MemberSubscription>(), chapterId)
        .InChapter(chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds) => Set()
        .Current(Set<MemberSubscription>(), chapterId)
        .InChapter(chapterId)
        .Where(x => memberIds.Contains(x.Id))
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetLatestByChapterId(Guid chapterId, int pageSize) => Set()
        .Current(Set<MemberSubscription>(), chapterId)
        .InChapter(chapterId)
        .OrderByDescending(x => x.CreatedUtc)
        .Take(pageSize)
        .DeferredMultiple();

    protected override IQueryable<Member> Set() => base.Set()
        .Include(x => x.Chapters);
}
