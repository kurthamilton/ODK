using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactAdminService
{
    Task<MessagesAdminPageViewModel> GetMessagesViewModel(Guid currentMemberId);

    Task<MessageAdminPageViewModel> GetMessageViewModel(Guid currentMemberId, Guid messageId);

    Task<ServiceResult> ReplyToMessage(IMemberServiceRequest request, Guid messageId, string message);

    Task<ServiceResult> SetMessageAsReplied(Guid currentMemberId, Guid messageId);
}