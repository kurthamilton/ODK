using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(
        MemberServiceRequest request, Guid eventId, Chapter chapter, string comment, Guid? parentEventCommentId);

    Task CompleteEventTicketPurchase(Guid eventId, Guid memberId);

    Task<ServiceResult> JoinWaitingList(Guid eventId, Guid memberId);

    Task<ServiceResult> LeaveWaitingList(Guid eventId, Guid memberId);

    Task NotifyWaitingList(ServiceRequest request, Guid eventId);

    Task<ServiceResult> UpdateMemberResponse(MemberServiceRequest request, Guid eventId, EventResponseType responseType);
}