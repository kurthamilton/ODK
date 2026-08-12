using Microsoft.EntityFrameworkCore;
using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Emails;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterEmailRepository : ReadWriteRepositoryBase<ChapterEmail>, IChapterEmailRepository
{
    public ChapterEmailRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterEmail> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<ChapterEmail> GetByChapterId(Guid chapterId, EmailType type) => Set()
        .Where(x => x.ChapterId == chapterId && x.Type == type)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingle<ChapterEmailDto> GetDto(Guid? chapterId, EmailType type)
    {
        /* The two site rows are joined rather than looked up separately so the whole send comes back in one
           query. A null chapterId matches no override, which is how a send with no group gets the site's
           templates. */
        var query =
            from email in Set<Email>().Where(x => x.Type == type)
            from layout in Set<Email>().Where(x => x.Type == EmailType.Layout)
            select new ChapterEmailDto
            {
                ChapterEmail = Set<ChapterEmail>()
                    .FirstOrDefault(x => x.ChapterId == chapterId && x.Type == type),
                ChapterLayout = Set<ChapterEmail>()
                    .FirstOrDefault(x => x.ChapterId == chapterId && x.Type == EmailType.Layout),
                SiteEmail = email,
                SiteLayout = layout
            };

        return query.DeferredSingle();
    }
}