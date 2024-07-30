using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(Guid currentMemberId, Guid eventId, string comment, Guid? parentEventCommentId);

    Task<EventCommentsDto> GetCommentsDto(Member? member, Event @event);

    Task<Event> GetEvent(Guid chapterId, Guid eventId);

    Task<EventDto> GetEventDto(Member? member, Event @event);

    Task<EventResponsesDto> GetEventResponsesDto(Event @event);

    Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Chapter chapter);

    Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId, 
        DateTime? afterUtc);
    
    Task<ServiceResult> UpdateMemberResponse(Member member, Guid eventId, EventResponseType responseType);
}
