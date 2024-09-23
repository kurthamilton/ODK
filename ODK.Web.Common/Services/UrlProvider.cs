using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

public class UrlProvider : IUrlProvider
{
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly PlatformType _platform;

    public UrlProvider(
        IHttpRequestProvider httpRequestProvider,
        IPlatformProvider platformProvider)
    {
        _httpRequestProvider = httpRequestProvider;
        _platform = platformProvider.GetPlatform();
    }

    public string ActivateAccountUrl(Chapter? chapter, string token)
        => GetUrl(OdkRoutes.Account.Activate(chapter, token));

    public string BaseUrl()
        => GetUrl("");

    public string ConfirmEmailAddressUpdate(Chapter? chapter, string token)
        => GetUrl(OdkRoutes.Account.EmailAddressChangeConfirm(chapter, token));

    public string ConversationAdminUrl(Chapter chapter, Guid conversationId)
        => GetUrl(OdkRoutes.MemberGroups.GroupConversation(_platform, chapter, conversationId));

    public string ConversationUrl(Chapter chapter, Guid conversationId)
        => GetUrl(OdkRoutes.Groups.Conversation(_platform, chapter, conversationId));

    public string EmailPreferences(Chapter? chapter)
        => GetUrl(OdkRoutes.Account.EmailPreferences(chapter));

    public string EventRsvpUrl(Chapter chapter, Guid eventId)
        => GetUrl($"/events/{eventId}/attend");

    public string EventsUrl(Chapter chapter) 
        => GetUrl(OdkRoutes.Groups.Events(_platform, chapter));

    public string EventUrl(Chapter chapter, Guid eventId)
        => GetUrl(OdkRoutes.Groups.Event(_platform, chapter, eventId));

    public string GroupUrl(Chapter chapter)
        => GetUrl(OdkRoutes.Groups.Group(_platform, chapter));

    public string GroupsUrl()
        => GetUrl(OdkRoutes.Groups.Index(_platform));

    public string MemberAdminUrl(Chapter chapter, Guid memberId)
        => GetUrl(OdkRoutes.MemberGroups.Member(_platform, chapter, memberId));

    public string MemberSiteSubscriptionUrl()
        => GetUrl(OdkRoutes.Account.Subscription(_platform, null));

    public string MessageAdminUrl(Guid messageId)
        => GetUrl($"/superadmin/messages/{messageId}");

    public string MessageAdminUrl(Chapter chapter, Guid messageId) 
        => GetUrl(OdkRoutes.MemberGroups.GroupMessage(_platform, chapter, messageId));

    public string PasswordReset(Chapter? chapter, string token)
        => GetUrl(OdkRoutes.Account.PasswordReset(chapter, token));

    public string TopicApprovalUrl()
        => GetUrl("/superadmin/topics");

    private string GetUrl(string path) 
        => $"{UrlUtils.BaseUrl(_httpRequestProvider.RequestUrl)}{path}";
}
