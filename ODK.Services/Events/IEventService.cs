using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(
        MemberServiceRequest request, Guid eventId, Chapter chapter, string comment, Guid? parentEventCommentId);

    Task CompleteEventTicketPurchase(Guid eventId, Guid memberId);

    Task<ServiceResult> JoinWaitlist(Guid eventId, Guid memberId);

    Task<ServiceResult> LeaveWaitlist(Guid eventId, Guid memberId);

    Task NotifyWaitlist(ServiceRequest request, Guid eventId);

    Task<ServiceResult> UpdateMemberResponse(MemberServiceRequest request, Guid eventId, EventResponseType responseType);
}