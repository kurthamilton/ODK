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

    public UrlProvider(IHttpRequestProvider httpRequestProvider)
    {
        _httpRequestProvider = httpRequestProvider;
    }

    public string ConversationAdminUrl(PlatformType platform, Chapter chapter, Guid conversationId)
        => GetUrl(OdkRoutes2.MemberGroups.GroupConversation(platform, chapter, conversationId));

    public string ConversationUrl(PlatformType platform, Chapter chapter, Guid conversationId)
        => GetUrl(OdkRoutes2.Groups.Conversation(platform, chapter, conversationId));

    public string EventsUrl(PlatformType platform, Chapter chapter) 
        => GetUrl(OdkRoutes.Chapters.Events(platform, chapter));

    public string MessageAdminUrl(PlatformType platform, Guid messageId)
        => GetUrl($"/superadmin/messages/{messageId}");

    public string MessageAdminUrl(PlatformType platform, Chapter chapter, Guid messageId) 
        => GetUrl(OdkRoutes2.MemberGroups.GroupMessage(platform, chapter, messageId));

    private string GetUrl(string path) 
        => $"{UrlUtils.BaseUrl(_httpRequestProvider.RequestUrl)}{path}";
}
