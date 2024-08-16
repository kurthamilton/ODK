using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(Guid currentMemberId, Guid eventId, string comment, Guid? parentEventCommentId);

    Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId, 
        DateTime? afterUtc);

    Task<ServiceResult> PayDeposit(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> PayTicketRemainder(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> PurchaseTicket(Guid currentMemberId, Guid eventId, string cardToken);

    Task<ServiceResult> UpdateMemberResponse(Guid currentMemberId, Guid eventId, EventResponseType responseType);
}
