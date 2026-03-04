using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterConversationQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterConversation, IChapterConversationQueryBuilder>, IChapterConversationQueryBuilder
{
    public ChapterConversationQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IChapterConversationQueryBuilder Builder => this;

    public IChapterConversationQueryBuilder Archived(bool value)
    {
        Query = Query.Where(x => x.ArchivedUtc != null == value);
        return this;
    }

    public IChapterQueryBuilder Chapter()
    {
        var query =
            from conversation in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == conversation.ChapterId)
            select chapter;

        return CreateQueryBuilder<IChapterQueryBuilder, Chapter>(
            context => new ChapterQueryBuilder(context, query));
    }

    public IChapterConversationQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterConversationQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IChapterConversationDtoQueryBuilder ToDto() =>
        CreateQueryBuilder<IChapterConversationDtoQueryBuilder, ChapterConversationDto>(
            context => new ChapterConversationDtoQueryBuilder(context, Query));
}