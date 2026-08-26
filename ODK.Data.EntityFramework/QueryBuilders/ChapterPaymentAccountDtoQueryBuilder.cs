using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterPaymentAccountDtoQueryBuilder
    : QueryBuilder<ChapterPaymentAccountDto>, IQueryBuilder<ChapterPaymentAccountDto>,
    IChapterPaymentAccountDtoQueryBuilder
{
    public ChapterPaymentAccountDtoQueryBuilder(
        DbContext context, IQueryable<ChapterPaymentAccount> query)
        : base(context, BaseQuery(context, query))
    {
    }

    private static IQueryable<ChapterPaymentAccountDto> BaseQuery(
        DbContext context, IQueryable<ChapterPaymentAccount> chapterPaymentAccountQuery)
    {
        var query =
            from chapterPaymentAccount in chapterPaymentAccountQuery
            from sitePaymentSettings in context.Set<SitePaymentSettings>()
                .Where(x => x.Id == chapterPaymentAccount.SitePaymentSettingId)
            select new ChapterPaymentAccountDto
            {
                ChapterPaymentAccount = chapterPaymentAccount,
                SitePaymentSettings = sitePaymentSettings
            };

        return query;
    }
}
