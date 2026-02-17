using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberAvatarRepository : WriteRepositoryBase<MemberAvatar>, IMemberAvatarRepository
{
    public MemberAvatarRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberAvatar> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberAvatarVersionDto> GetVersionDtoByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .Select(x => new MemberAvatarVersionDto
            {
                MemberId = x.MemberId,
                Version = x.VersionInt
            })
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<MemberAvatarVersionDto> GetVersionDtosByChapterId(Guid chapterId)
    {
        var query =
            from member in Set<Member>()
                .InChapter(chapterId)
            from avatar in Set()
                .Where(x => x.MemberId == member.Id)
            select new MemberAvatarVersionDto
            {
                MemberId = member.Id,
                Version = avatar.VersionInt
            };

        return query.DeferredMultiple();
    }
}