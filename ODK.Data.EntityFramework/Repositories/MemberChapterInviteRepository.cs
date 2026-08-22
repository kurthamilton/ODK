using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberChapterInviteRepository : WriteRepositoryBase<MemberChapterInvite>, IMemberChapterInviteRepository
{
    public MemberChapterInviteRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberChapterInvite> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<MemberChapterInvite> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByMemberId(Guid memberId, Guid chapterId) => Set()
        .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberChapterInvite> GetByToken(string token) => Set()
        .Where(x => x.Token == token)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<MemberChapterInviteDto> GetDtosByChapterId(Guid chapterId) =>
        (from invite in Set()
         join member in Set<Member>() on invite.MemberId equals member.Id
         where invite.ChapterId == chapterId
         orderby invite.CreatedUtc
         select new MemberChapterInviteDto
         {
             CreatedUtc = invite.CreatedUtc,
             Member = member
         })
        .DeferredMultiple();
}
