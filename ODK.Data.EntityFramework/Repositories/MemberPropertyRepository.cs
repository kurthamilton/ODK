using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class MemberPropertyRepository : ReadWriteRepositoryBase<MemberProperty>, IMemberPropertyRepository
{
    public MemberPropertyRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberProperty> GetByMemberId(Guid memberId, Guid chapterId)
    {
        var query =
            from memberProperty in Set().Where(x => x.MemberId == memberId)
            from chapterProperty in Set<ChapterProperty>()
            where memberProperty.ChapterPropertyId == chapterProperty.Id
                && chapterProperty.ChapterId == chapterId
            select memberProperty;
        return query.DeferredMultiple();
    }
}
