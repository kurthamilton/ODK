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
        .OrderBy(x => x.Order)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EmailProviderSummaryDto> GetEmailsSentToday(Guid chapterId)
    {
        var query =
            from provider in Set()
            from sentEmail in Set<SentEmail>()
            where sentEmail.ChapterEmailProviderId == provider.Id &&
                sentEmail.SentUtc >= DateTime.UtcNow.Date &&
                sentEmail.SentUtc < DateTime.UtcNow.Date.AddDays(1)
            group provider by provider.Id into g
            select new EmailProviderSummaryDto
            {
                Sent = g.Count(),
                EmailProviderId = g.Key
            };

        return query.DeferredMultiple();
    }
}
