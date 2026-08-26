using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterPaymentAccountQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>,
    IChapterPaymentAccountQueryBuilder
{
    public ChapterPaymentAccountQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterPaymentAccountQueryBuilder Builder => this;

    public IChapterPaymentAccountQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterPaymentAccountDtoQueryBuilder ToDto() =>
        CreateQueryBuilder<IChapterPaymentAccountDtoQueryBuilder, ChapterPaymentAccountDto>(
            context => new ChapterPaymentAccountDtoQueryBuilder(context, Query));

    public ISitePaymentSettingsQueryBuilder ToSitePaymentSettings()
    {
        Func<DbContext, IQueryable<SitePaymentSettings>> query = context =>
            from chapterPaymentAccount in Query
            from sitePaymentSettings in context.Set<SitePaymentSettings>()
                .Where(x => x.Id == chapterPaymentAccount.SitePaymentSettingId)
            select sitePaymentSettings;

        return CreateQueryBuilder<ISitePaymentSettingsQueryBuilder, SitePaymentSettings>(
            context => new SitePaymentSettingsQueryBuilder(context, query(context)));
    }
}
