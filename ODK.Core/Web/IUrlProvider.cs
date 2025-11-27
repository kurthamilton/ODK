using ODK.Core.Chapters;

namespace ODK.Core.Web;

public interface IUrlProvider
{
    string ActivateAccountUrl(IHttpRequestContext httpRequestContext, Chapter? chapter, string token);

    string BaseUrl(IHttpRequestContext httpRequestContext);

    string ConfirmEmailAddressUpdate(IHttpRequestContext httpRequestContext, Chapter? chapter, string token);

    string ConversationAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid conversationId);

    string ConversationUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid conversationId);

    string EmailPreferences(IHttpRequestContext httpRequestContext, Chapter? chapter);

    string EventRsvpUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid eventId);

    string EventsUrl(IHttpRequestContext httpRequestContext, Chapter chapter);    

    string EventUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid eventId);

    string GroupUrl(IHttpRequestContext httpRequestContext, Chapter chapter);

    string GroupsUrl(IHttpRequestContext httpRequestContext);

    string IssueAdminUrl(IHttpRequestContext httpRequestContext, Guid issueId);

    string IssueUrl(IHttpRequestContext httpRequestContext, Guid issueId);

    string MemberAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid memberId);

    string MemberSiteSubscriptionUrl(IHttpRequestContext httpRequestContext);

    string MessageAdminUrl(IHttpRequestContext httpRequestContext, Guid messageId);

    string MessageAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid messageId);

    string PasswordReset(IHttpRequestContext httpRequestContext, Chapter? chapter, string token);

    string TopicApprovalUrl(IHttpRequestContext httpRequestContext);
}
