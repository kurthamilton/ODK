using ODK.Core.Messages;
using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactAdminService
{
    Task<ServiceResult> DeleteSpamMessages(IMemberServiceRequest request);

    Task<MessagesAdminPageViewModel> GetMessagesViewModel(IMemberServiceRequest request, MessageStatus status);

    Task<MessageAdminPageViewModel> GetMessageViewModel(IMemberServiceRequest request, Guid messageId);

    Task<ServiceResult> ReplyToMessage(IMemberServiceRequest request, Guid messageId, string message);

    Task<ServiceResult> SetMessageAsReplied(IMemberServiceRequest request, Guid messageId);
}