using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterConversationRepository : IReadWriteRepository<ChapterConversation, IChapterConversationQueryBuilder>
{
    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, bool readByChapter);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId);
}