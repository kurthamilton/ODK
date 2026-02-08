using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
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

    public IDeferredQueryMultiple<Member> GetAllByChapterId(Guid chapterId) => Set()
        .InChapter(chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId) => Set()
        .Current(chapterId)
        .Visible(chapterId)
        .InChapter(chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds) => Set()
        .Current(chapterId)
        .InChapter(chapterId)
        .Where(x => memberIds.Contains(x.Id))
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress) => Set()
        .Where(x => x.EmailAddress == emailAddress)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingle<Member> GetChapterOwner(Guid chapterId)
    {
        var query =
            from member in Set()
            from chapter in Set<Chapter>()
                .Where(x => member.Id == x.OwnerId)
            where chapter.Id == chapterId
            select member;

        return query.DeferredSingle();
    }

    public IDeferredQuery<int> GetCountByChapterId(Guid chapterId) => Set()
        .Current(chapterId)
        .Visible(chapterId)
        .InChapter(chapterId)
        .DeferredCount();

    public IDeferredQueryMultiple<Member> GetLatestByChapterId(Guid chapterId, int pageSize)
    {
        var query =
            from member in Set()
                .Current(chapterId)
                .Visible(chapterId)
            from memberChapter in Set<MemberChapter>()
            where memberChapter.MemberId == member.Id
                && memberChapter.ChapterId == chapterId
            orderby memberChapter.CreatedUtc descending
            select member;

        return query
            .Take(pageSize)
            .DeferredMultiple();
    }

    protected override IQueryable<Member> Set() => base.Set()
        .Include(x => x.Chapters);
}