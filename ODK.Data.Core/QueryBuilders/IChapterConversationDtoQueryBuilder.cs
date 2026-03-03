using ODK.Core.Messages;
using ODK.Data.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterConversationDtoQueryBuilder : IQueryBuilder<ChapterConversationDto>
{
    IChapterConversationDtoQueryBuilder ForStatus(MessageStatus status);
}
