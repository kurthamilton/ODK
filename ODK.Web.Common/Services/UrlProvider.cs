using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProvider : IUrlProvider
{
    private readonly IPlatformProvider _platformProvider;

    public UrlProvider(IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    public string ActivateAccountUrl(IHttpRequestContext httpRequestContext, Chapter? chapter, string token)
        => GetUrl(httpRequestContext, OdkRoutes.Account.Activate(chapter, token));

    public string BaseUrl(IHttpRequestContext httpRequestContext)
        => GetUrl(httpRequestContext, "");

    public string ConfirmEmailAddressUpdate(IHttpRequestContext httpRequestContext, Chapter? chapter, string token)
        => GetUrl(httpRequestContext, OdkRoutes.Account.EmailAddressChangeConfirm(chapter, token));

    public string ConversationAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid conversationId)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.MemberGroups.GroupConversation(_platformProvider.GetPlatform(httpRequestContext), chapter, conversationId));

    public string ConversationUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid conversationId)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Groups.Conversation(_platformProvider.GetPlatform(httpRequestContext), chapter, conversationId));

    public string EmailPreferences(IHttpRequestContext httpRequestContext, Chapter? chapter)
        => GetUrl(httpRequestContext, OdkRoutes.Account.EmailPreferences(chapter));

    public string EventRsvpUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid eventId)
        => GetUrl(httpRequestContext, $"/events/{eventId}/attend");

    public string EventsUrl(IHttpRequestContext httpRequestContext, Chapter chapter) 
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Groups.Events(_platformProvider.GetPlatform(httpRequestContext), chapter));

    public string EventUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid eventId)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Groups.Event(_platformProvider.GetPlatform(httpRequestContext), chapter, eventId));

    public string GroupUrl(IHttpRequestContext httpRequestContext, Chapter chapter)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Groups.Group(_platformProvider.GetPlatform(httpRequestContext), chapter));

    public string GroupsUrl(IHttpRequestContext httpRequestContext)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Groups.Index(_platformProvider.GetPlatform(httpRequestContext)));

    public string IssueAdminUrl(IHttpRequestContext httpRequestContext, Guid issueId)
        => GetUrl(httpRequestContext, $"/superadmin/issues/{issueId}");

    public string IssueUrl(IHttpRequestContext httpRequestContext, Guid issueId)
        => GetUrl(httpRequestContext, OdkRoutes.Account.Issue(issueId));

    public string MemberAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid memberId)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.MemberGroups.Member(_platformProvider.GetPlatform(httpRequestContext), chapter, memberId));

    public string MemberSiteSubscriptionUrl(IHttpRequestContext httpRequestContext)
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.Account.Subscription(_platformProvider.GetPlatform(httpRequestContext), null));

    public string MessageAdminUrl(IHttpRequestContext httpRequestContext, Guid messageId)
        => GetUrl(httpRequestContext, $"/superadmin/messages/{messageId}");

    public string MessageAdminUrl(IHttpRequestContext httpRequestContext, Chapter chapter, Guid messageId) 
        => GetUrl(
            httpRequestContext, 
            OdkRoutes.MemberGroups.GroupMessage(_platformProvider.GetPlatform(httpRequestContext), chapter, messageId));

    public string PasswordReset(IHttpRequestContext httpRequestContext, Chapter? chapter, string token)
        => GetUrl(httpRequestContext, OdkRoutes.Account.PasswordReset(chapter, token));

    public string TopicApprovalUrl(IHttpRequestContext httpRequestContext)
        => GetUrl(httpRequestContext, "/superadmin/topics");

    private string GetUrl(IHttpRequestContext httpRequestContext, string path) 
        => $"{UrlUtils.BaseUrl(httpRequestContext.RequestUrl)}{path}";
}
