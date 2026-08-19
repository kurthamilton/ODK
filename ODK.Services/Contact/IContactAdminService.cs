using ODK.Core.Messages;
using ODK.Services.Contact.ViewModels;

namespace ODK.Services.Contact;

public interface IContactAdminService
{
    Task<ServiceResult> DeleteSpamMessages(IMemberServiceRequest request);

    Task<MessagesAdminPageViewModel> GetMessagesViewModel(IMemberServiceRequest request, MessageStatus status);

    Task<MessageAdminPageViewModel> GetMessageViewModel(IMemberServiceRequest request, Guid messageId);

    Task<SiteConversationAdminPageViewModel> GetSiteConversationViewModel(
        IMemberServiceRequest request, Guid conversationId);

    Task<SiteConversationsAdminPageViewModel> GetSiteConversationsViewModel(
        IMemberServiceRequest request, bool archived);

    Task<ServiceResult> ReplyToMessage(IMemberServiceRequest request, Guid messageId, string message);

    Task<ServiceResult> ReplyToSiteConversation(
        IMemberServiceRequest request, Guid conversationId, string message);

    Task<ServiceResult> SetMessageAsReplied(IMemberServiceRequest request, Guid messageId);
}