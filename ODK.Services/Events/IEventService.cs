using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(MemberServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId);

    Task<(Chapter, Event)> GetEvent(Guid eventId);

    Task<ServiceResult> PayDeposit(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> PayTicketRemainder(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> PurchaseTicket(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> UpdateMemberResponse(Guid currentMemberId, Guid eventId, EventResponseType responseType);
}
