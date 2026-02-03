using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventCommentRepository : ReadWriteRepositoryBase<EventComment>, IEventCommentRepository
{
    public EventCommentRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventComment> GetByEventId(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.CreatedUtc)
            .DeferredMultiple();

    public IDeferredQueryMultiple<EventComment> GetByEventShortcode(string shortcode)
    {
        var query =
            from comment in Set()
            from @event in Set<Event>()
                .Where(x => x.Id == comment.EventId)
            where @event.Shortcode == shortcode
            select comment;

        return query
            .OrderBy(x => x.CreatedUtc)
            .DeferredMultiple();
    }

    public IDeferredQuerySingle<EventComment> GetById(Guid id, bool includeHidden)
        => (includeHidden ? Set<EventComment>() : Set())
            .Where(x => x.Id == id)
            .DeferredSingle();

    public IDeferredQueryMultiple<EventCommentDto> GetDtosByEventId(Guid eventId, bool includeHidden)
    {
        var commentQuery = includeHidden
            ? Set<EventComment>()
            : Set();

        var query =
            from comment in commentQuery
            from member in Set<Member>()
                .Where(x => x.Id == comment.MemberId)
            where comment.EventId == eventId
            select new EventCommentDto
            {
                Comment = comment,
                Member = member
            };

        return query
            .OrderBy(x => x.Comment.CreatedUtc)
            .DeferredMultiple();
    }

    protected override IQueryable<EventComment> Set()
        => base.Set()
            .Where(x => !x.Hidden);
}