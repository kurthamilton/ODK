using ODK.Core.Members;
using ODK.Data.Core.Deferred;
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

    public IDeferredQueryMultiple<MemberAvatar> GetByChapterId(Guid chapterId)
    {
        var query =
            from memberImage in Set()
            from member in Set<Member>().InChapter(chapterId)
            where member.Id == memberImage.MemberId
            select memberImage;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<MemberAvatar> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();
}
