using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterConversationMessageRepository : ReadWriteRepositoryBase<ChapterConversationMessage>, IChapterConversationMessageRepository
{
    public ChapterConversationMessageRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterConversationMessage> GetByConversationId(Guid chapterConversationId) => Set()
        .Where(x => x.ChapterConversationId == chapterConversationId)
        .DeferredMultiple();

    protected override IQueryable<ChapterConversationMessage> Set() => base.Set()
        .Include(x => x.Member)
        .ThenInclude(x => x!.Chapters);
}
