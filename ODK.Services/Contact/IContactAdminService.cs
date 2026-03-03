using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactAdminService
{
    Task<MessagesAdminPageViewModel> GetMessagesViewModel(IMemberServiceRequest request, bool spam);

    Task<MessageAdminPageViewModel> GetMessageViewModel(IMemberServiceRequest request, Guid messageId);

    Task<ServiceResult> ReplyToMessage(IMemberServiceRequest request, Guid messageId, string message);

    Task<ServiceResult> SetMessageAsReplied(IMemberServiceRequest request, Guid messageId);
}