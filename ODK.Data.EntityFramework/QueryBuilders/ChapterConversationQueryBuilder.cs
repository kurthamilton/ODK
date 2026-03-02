using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Messages;
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
