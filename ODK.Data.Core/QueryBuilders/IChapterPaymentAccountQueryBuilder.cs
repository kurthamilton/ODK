using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterPaymentAccountQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterPaymentAccount, IChapterPaymentAccountQueryBuilder>
{
    IChapterPaymentAccountQueryBuilder ForChapter(Guid chapterId);

    IChapterPaymentAccountQueryBuilder ForEnvironment(EnvironmentType environment);
}
