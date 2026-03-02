using ODK.Core.Chapters;
using ODK.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterConversationQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterConversation, IChapterConversationQueryBuilder>
{
    IChapterConversationQueryBuilder ForChapter(Guid chapterId);

    IChapterConversationQueryBuilder ForMember(Guid memberId);

    IChapterConversationDtoQueryBuilder ToDto();
}