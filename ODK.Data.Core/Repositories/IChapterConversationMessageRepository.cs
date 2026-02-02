using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterConversationMessageRepository : IReadWriteRepository<ChapterConversationMessage>
{
    IDeferredQueryMultiple<ChapterConversationMessage> GetByConversationId(Guid chapterConversationId);

    IDeferredQueryMultiple<ChapterConversationMessageDto> GetDtosByConversationId(Guid chapterConversationId);
}
