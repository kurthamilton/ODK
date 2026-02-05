using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventService
{
    Task<ServiceResult> AddComment(
        IMemberChapterServiceRequest request, Guid eventId, string comment, Guid? parentEventCommentId);

    Task CompleteEventTicketPurchase(Guid eventId, Guid memberId);

    Task<Event> GetById(Guid eventId);

    Task<ServiceResult> JoinWaitlist(Guid eventId, Guid memberId);

    Task<ServiceResult> LeaveWaitlist(Guid eventId, Guid memberId);

    Task NotifyWaitlist(IServiceRequest request, Guid eventId);

    Task<ServiceResult> UpdateMemberResponse(
        IMemberServiceRequest request, Guid eventId, EventResponseType responseType, Guid? adminMemberId);

    Task<ServiceResult> UpdateMemberResponse(
        IMemberServiceRequest request, string shortcode, EventResponseType responseType, Guid? adminMemberId);
}