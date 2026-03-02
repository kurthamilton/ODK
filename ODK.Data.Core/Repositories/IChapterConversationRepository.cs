using ODK.Core.Chapters;
using ODK.Core.Messages;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterConversationRepository : IReadWriteRepository<ChapterConversation, IChapterConversationQueryBuilder>
{
    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByChapterId(Guid chapterId, MessageStatus status);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId);

    IDeferredQueryMultiple<ChapterConversationDto> GetDtosByMemberId(Guid memberId, Guid chapterId);
}