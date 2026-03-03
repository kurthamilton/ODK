using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterContactMessageQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterContactMessage, IChapterContactMessageQueryBuilder>, IChapterContactMessageQueryBuilder
{
    public ChapterContactMessageQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterContactMessageQueryBuilder Builder => this;

    public IChapterContactMessageQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterContactMessageQueryBuilder ForSpamScore(bool isSpam, double threshold)
    {
        Query = Query
            .Where(x => isSpam
                ? x.RecaptchaScore < threshold
                : x.RecaptchaScore >= threshold);
        return this;
    }
}