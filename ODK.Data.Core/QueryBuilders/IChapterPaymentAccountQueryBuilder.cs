using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterPaymentAccountQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>
{
    IChapterPaymentAccountQueryBuilder ForChapter(Guid chapterId);

    IChapterPaymentAccountDtoQueryBuilder ToDto();

    ISitePaymentSettingsQueryBuilder ToSitePaymentSettings();
}
