using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(MemberServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId);

    Task CompleteEventTicketPurchase(Guid eventId, Guid memberId);

    Task<(Chapter, Event)> GetEvent(Guid eventId);

    Task<ServiceResult> UpdateMemberResponse(Guid currentMemberId, Guid eventId, EventResponseType responseType);
}