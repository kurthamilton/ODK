using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Core.Web;

public interface IUrlProvider
{
    string ActivateAccountUrl(Chapter? chapter, string token);

    string ConfirmEmailAddressUpdate(Chapter? chapter, string token);

    string ConversationAdminUrl(Chapter chapter, Guid conversationId);

    string ConversationUrl(Chapter chapter, Guid conversationId);

    string EmailPreferences(Chapter? chapter);

    string EventRsvpUrl(Chapter chapter, Guid eventId, EventResponseType response);

    string EventsUrl(Chapter chapter);    

    string EventUrl(Chapter chapter, Guid eventId);

    string MessageAdminUrl(Guid messageId);

    string MessageAdminUrl(Chapter chapter, Guid messageId);

    string PasswordReset(Chapter? chapter, string token);
}
