using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterConversationDtoQueryBuilder : IQueryBuilder<ChapterConversationDto>
{
    IChapterConversationDtoQueryBuilder ForStatus(ChapterConversationStatus status);
}
