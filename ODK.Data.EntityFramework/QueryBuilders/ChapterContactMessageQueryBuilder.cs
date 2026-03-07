using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Messages;
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

    public IChapterContactMessageQueryBuilder ForStatus(MessageStatus status, double spamThreshold)
    {
        if (status == MessageStatus.Unreplied)
        {
            Query = Query.Where(x => x.RecaptchaScore >= spamThreshold && x.RepliedUtc == null);
        }
        else if (status == MessageStatus.Replied)
        {
            Query = Query.Where(x => x.RecaptchaScore >= spamThreshold && x.RepliedUtc != null);
        }
        else if (status == MessageStatus.Spam)
        {
            Query = Query.Where(x => x.RecaptchaScore < spamThreshold);
        }

        return this;
    }
}