using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(
        MemberChapterServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId);

    Task CompleteEventTicketPurchase(Guid eventId, Guid memberId);

    Task<Event> GetById(Guid eventId);

    Task<ServiceResult> JoinWaitlist(Guid eventId, Guid memberId);

    Task<ServiceResult> LeaveWaitlist(Guid eventId, Guid memberId);

    Task NotifyWaitlist(ServiceRequest request, Guid eventId);

    Task<ServiceResult> UpdateMemberResponse(
        MemberServiceRequest request, Guid eventId, EventResponseType responseType, Guid? adminMemberId);

    Task<ServiceResult> UpdateMemberResponse(
        MemberServiceRequest request, string shortcode, EventResponseType responseType, Guid? adminMemberId);
}