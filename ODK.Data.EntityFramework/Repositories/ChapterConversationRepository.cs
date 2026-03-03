using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterConversationRepository :
    ReadWriteRepositoryBase<ChapterConversation, IChapterConversationQueryBuilder>, IChapterConversationRepository
{
    public ChapterConversationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .ForMember(memberId)
            .ToDto()
            .GetAll();

    public override IChapterConversationQueryBuilder Query()
        => CreateQueryBuilder(context => new ChapterConversationQueryBuilder(context));
}