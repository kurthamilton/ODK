using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterEmailProviderRepository : ReadWriteRepositoryBase<ChapterEmailProvider>, IChapterEmailProviderRepository
{
    public ChapterEmailProviderRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterEmailProvider> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterEmailProviderSummaryDto> GetEmailsSentToday(Guid chapterId)
    {
        var query =
            from emailProvider in Set()
            from sentEmail in Set<SentEmail>()
            where emailProvider.ChapterId == chapterId && 
                sentEmail.ChapterEmailProviderId == emailProvider.Id && 
                sentEmail.SentDate >= DateTime.Today &&
                sentEmail.SentDate < DateTime.Today.AddDays(1)
            group emailProvider by emailProvider.Id into g
            select new ChapterEmailProviderSummaryDto
            {
                ChapterEmailProviderId = g.Key,
                Sent = g.Count()
            };

        return query.DeferredMultiple();
    }
}
