using System;
using ODK.Core.Chapters;
using ODK.Core.Web;
using ODK.Services;
using ODK.Services.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProvider : IUrlProvider
{
    private readonly IHttpRequestContext _httpRequestContext;
    private readonly IOdkRoutes _odkRoutes;
    
    public UrlProvider(ServiceRequest request, IOdkRoutes odkRoutes)
    {
        _httpRequestContext = request.HttpRequestContext;
        _odkRoutes = odkRoutes;
    }

    public string ActivateAccountUrl(Chapter? chapter, string token)
        => GetUrl(_odkRoutes.Account.Activate(chapter, token));

    public string BaseUrl() => GetUrl(string.Empty);

    public string ConfirmEmailAddressUpdate(Chapter? chapter, string token)
        => GetUrl(_odkRoutes.Account.EmailAddressChangeConfirm(chapter, token));

    public string ConversationAdminUrl(Chapter chapter, Guid conversationId)
        => GetUrl(_odkRoutes.GroupAdmin.Conversation(chapter, conversationId));

    public string ConversationUrl(Chapter chapter, Guid conversationId) 
        => GetUrl(_odkRoutes.Groups.Conversation(chapter, conversationId));

    public string EmailPreferences(Chapter? chapter) => GetUrl(_odkRoutes.Account.EmailPreferences(chapter));

    public string EventRsvpUrl(Chapter chapter, string shortcode) 
        => GetUrl(_odkRoutes.Groups.EventAttend(chapter, shortcode));

    public string EventsUrl(Chapter chapter) => GetUrl(_odkRoutes.Groups.Events(chapter));

    public string EventUrl(Chapter chapter, string shortcode) 
        => GetUrl(_odkRoutes.Groups.Event(chapter, shortcode));

    public string GroupUrl(Chapter chapter) => GetUrl(_odkRoutes.Groups.Group(chapter));

    public string GroupsUrl() => GetUrl(_odkRoutes.Groups.Index());

    public string IssueAdminUrl(Guid issueId) => GetUrl(_odkRoutes.SiteAdmin.Issue(issueId));

    public string IssueUrl(Guid issueId) => GetUrl(_odkRoutes.Account.Issue(issueId));

    public string LoginUrl(Chapter? chapter) => GetUrl(_odkRoutes.Account.Login(chapter));

    public string MemberAdminUrl(Chapter chapter, Guid memberId) 
        => GetUrl(_odkRoutes.GroupAdmin.Member(chapter, memberId));

    public string MemberSiteSubscriptionUrl() => GetUrl(_odkRoutes.Account.Subscription(null));

    public string MessageAdminUrl(Chapter chapter, Guid messageId) 
        => GetUrl(_odkRoutes.GroupAdmin.Message(chapter, messageId));

    public string MessageSiteAdminUrl(Guid messageId) => GetUrl(_odkRoutes.SiteAdmin.Message(messageId));

    public string PasswordReset(Chapter? chapter, string token) => GetUrl(_odkRoutes.Account.PasswordReset(chapter, token));

    public string SiteAdminGroups() => GetUrl(_odkRoutes.SiteAdmin.Groups);

    public string TopicApprovalUrl() => GetUrl(_odkRoutes.SiteAdmin.Topics);

    private string GetUrl(string path)
        => $"{_httpRequestContext.BaseUrl}{path}";

    private string GetUrl(GroupAdminRoute adminRoute) => GetUrl(adminRoute.Path);
}