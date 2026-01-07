using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterConversationMessageRepository : ReadWriteRepositoryBase<ChapterConversationMessage>, IChapterConversationMessageRepository
{
    private readonly OdkContext _context;

    public ChapterConversationMessageRepository(OdkContext context)
        : base(context)
    {
        _context = context;
    }

    public IDeferredQueryMultiple<ChapterConversationMessage> GetByConversationId(Guid chapterConversationId) => Set()
        .Where(x => x.ChapterConversationId == chapterConversationId)
        .DeferredMultiple();

    protected override IQueryable<ChapterConversationMessage> Set() => base.Set()
        .Include(x => x.Member)
        .ThenInclude(x => x!.Chapters);

    public override void Update(ChapterConversationMessage entity)
    {
        // Clone without member to prevent change tracker including Member and all descendant properties
        // TODO: how to update only top-level entity and ignore related properties?
        var copy = entity.Clone();
        copy.Member = null;

        base.Update(copy);
    }
}
