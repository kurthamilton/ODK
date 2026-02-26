using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterContactMessageReplyRepository : ReadWriteRepositoryBase<ChapterContactMessageReply>, IChapterContactMessageReplyRepository
{
    public ChapterContactMessageReplyRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterContactMessageReply> GetByChapterContactMessageId(Guid chapterContactMessageId) => Set()
        .Where(x => x.ChapterContactMessageId == chapterContactMessageId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterContactMessageReply> GetByChapterId(Guid chapterId)
    {
        var query =
            from message in Set<ChapterContactMessage>()
            from reply in Set<ChapterContactMessageReply>()
            where reply.ChapterContactMessageId == message.Id
                && message.ChapterId == chapterId
            select reply;

        return query.DeferredMultiple();
    }

    protected override IQueryable<ChapterContactMessageReply> Set() => base.Set()
        .Include(x => x.Member);
}