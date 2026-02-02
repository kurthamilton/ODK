using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterConversationMessageRepository : ReadWriteRepositoryBase<ChapterConversationMessage>, IChapterConversationMessageRepository
{
    public ChapterConversationMessageRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterConversationMessage> GetByConversationId(Guid chapterConversationId) 
        => Set()
            .Where(x => x.ChapterConversationId == chapterConversationId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterConversationMessageDto> GetDtosByConversationId(Guid chapterConversationId)
    {
        var query = 
            from message in Set()
            from member in Set<Member>()
                .Where(x => x.Id == message.MemberId)
            where message.ChapterConversationId == chapterConversationId
            select new ChapterConversationMessageDto
            {
                MemberFirstName = member.FirstName,
                MemberLastName = member.LastName,
                Message = message
            };
        
        return query.DeferredMultiple();
    }
}
