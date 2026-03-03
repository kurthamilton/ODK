using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterConversationQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterConversation, IChapterConversationQueryBuilder>
{
    IChapterConversationQueryBuilder Archived(bool value);

    IChapterQueryBuilder Chapter();

    IChapterConversationQueryBuilder ForChapter(Guid chapterId);

    IChapterConversationQueryBuilder ForMember(Guid memberId);

    IChapterConversationDtoQueryBuilder ToDto();
}