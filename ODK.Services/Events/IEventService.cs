using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<Event> GetEvent(Guid chapterId, Guid eventId);
    
    Task<EventResponsesDto> GetEventResponsesDto(Event @event);

    Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId);

    Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId, 
        DateTime? after);
    
    Task<ServiceResult> UpdateMemberResponse(Member member, Guid eventId, EventResponseType responseType);
}
