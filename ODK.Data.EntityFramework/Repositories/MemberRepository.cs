using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberRepository : ReadWriteRepositoryBase<Member, IMemberQueryBuilder>, IMemberRepository
{
    private readonly OdkContext _context;

    public MemberRepository(OdkContext context)
        : base(context)
    {
        _context = context;
    }

    public IDeferredQueryMultiple<Member> GetAllByChapterId(Guid chapterId)
        => Query()
            .InChapter(chapterId)
            .GetAll();

    public IDeferredQueryMultiple<MemberWithAvatarDto> GetAllWithAvatarByChapterId(Guid chapterId)
        => Query()
            .InChapter(chapterId)
            .WithAvatar()
            .GetAll();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId)
        => Query()
            .InChapter(chapterId)
            .GetAll();

    public IDeferredQueryMultiple<MemberWithAvatarDto> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds)
        => Query()
            .InChapter(chapterId)
            .ByIds(memberIds)
            .WithAvatar()
            .GetAll();

    public IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress)
        => Query()
            .HasEmailAddress(emailAddress)
            .GetSingleOrDefault();

    public IDeferredQuerySingle<Member> GetChapterOwner(Guid chapterId)
        => Query()
            .IsChapterOwner(chapterId)
            .GetSingle();

    public IDeferredQuery<int> GetCountByChapterId(Guid chapterId)
        => Query()
            .InChapter(chapterId)
            .Count();

    public IDeferredQueryMultiple<MemberWithAvatarDto> GetLatestWithAvatarByChapterId(Guid chapterId, int pageSize)
        => Query()
            .InChapter(chapterId)
            .Latest(8)
            .WithAvatar()
            .GetAll();

    public IDeferredQuerySingle<MemberWithAvatarDto> GetWithAvatarById(Guid memberId)
        => Query()
            .ById(memberId)
            .WithAvatar()
            .GetSingle();

    public override IMemberQueryBuilder Query() => new MemberQueryBuilder(_context);    
}