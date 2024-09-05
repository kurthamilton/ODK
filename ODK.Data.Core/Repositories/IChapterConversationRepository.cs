using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterConversationRepository : IReadWriteRepository<ChapterConversation>
{
    IDeferredQueryMultiple<ChapterConversation> GetByChapterId(Guid chapterId);    

    IDeferredQueryMultiple<ChapterConversation> GetByMemberId(Guid memberId, Guid chapterId);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, bool replied);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId);
}
