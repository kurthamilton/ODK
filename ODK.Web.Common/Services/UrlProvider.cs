using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Platforms;
using ODK.Services.Web;
using ODK.Web.Common.Routes;

namespace ODK.Web.Common.Services;

/// <summary>
/// Absolute URLs, each built against the platform that owns whatever it is about: a method that takes a
/// group takes the group's platform, and one that takes none takes the platform this was constructed with.
/// So a link to a Drunken Knitwits group points at Drunken Knitwits whichever site built it.
/// </summary>
/// <remarks>
/// The host is the platform's canonical URL from configuration, never the one the request arrived on. A URL
/// from here is read somewhere else - in an inbox, or by a payment provider - so it has to name the site
/// rather than whichever host happened to serve the request that built it. Nothing here renders a link into
/// a page, which is the one case where staying on the current host would matter.
/// </remarks>
public class UrlProvider : IUrlProvider
{
    private readonly IOdkRoutesFactory _odkRoutesFactory;
    private readonly PlatformType _platform;
    private readonly IPlatformProvider _platformProvider;

    /// <param name="platform">
    /// The platform a URL naming no group is built against. The served one for work about the site, and the
    /// group's own for work whose platform is decided by what it is about - see
    /// <see cref="IUrlProviderFactory"/>.
    /// </param>
    public UrlProvider(
        PlatformType platform,
        IOdkRoutesFactory odkRoutesFactory,
        IPlatformProvider platformProvider)
    {
        _odkRoutesFactory = odkRoutesFactory;
        _platform = platform;
        _platformProvider = platformProvider;
    }

    public string AcceptInviteUrl(Chapter chapter, string inviteToken)
        => GetUrl(chapter, x => x.Groups.AcceptInvite(chapter, inviteToken));

    public string ActivateAccountUrl(Chapter? chapter, string token)
        => GetUrl(chapter, x => x.Account.Activate(chapter, token));

    public string BaseUrl(Chapter? chapter) => GetUrl(chapter, _ => string.Empty);

    public string ChapterSubscription(Chapter chapter)
        => GetUrl(chapter, x => x.Groups.Subscription(chapter));

    public string ConfirmEmailAddressUpdate(Chapter? chapter, string token)
        => GetUrl(chapter, x => x.Account.EmailAddressChangeConfirm(chapter, token));

    public string ConversationAdminUrl(Chapter chapter, Guid conversationId)
        => GetUrl(chapter, x => x.GroupAdmin.Conversation(chapter, conversationId).Path);

    public string ConversationUrl(Chapter chapter, Guid conversationId)
        => GetUrl(chapter, x => x.Groups.Conversation(chapter, conversationId));

    public string EmailPreferences(Chapter? chapter)
        => GetUrl(chapter, x => x.Account.EmailPreferences(chapter));

    public string EventRsvpUrl(Chapter chapter, string shortcode)
        => GetUrl(chapter, x => x.Groups.EventAttend(chapter, shortcode));

    public string EventsUrl(Chapter chapter) => GetUrl(chapter, x => x.Groups.Events(chapter));

    public string EventUrl(Chapter chapter, string shortcode)
        => GetUrl(chapter, x => x.Groups.Event(chapter, shortcode));

    public string GroupUrl(Chapter chapter) => GetUrl(chapter, x => x.Groups.Group(chapter));

    public string GroupsUrl() => GetUrl(x => x.Groups.Index());

    public string InvitedMembersAdminUrl(Chapter chapter)
        => GetUrl(chapter, x => x.GroupAdmin.MembersInvited(chapter).Path);

    // Account.Create, not Account.Join: Join is chapter-scoped (/{chapter}/account/join), and a referral
    // is site-wide, so it points at the platform's own sign-up page.
    public string JoinUrl() => GetUrl(x => x.Account.Create());

    public string LoginUrl(Chapter? chapter) => GetUrl(chapter, x => x.Account.Login(chapter));

    public string MemberAdminUrl(Chapter chapter, Guid memberId)
        => GetUrl(chapter, x => x.GroupAdmin.Member(chapter, memberId).Path);

    public string MemberSiteSubscriptionUrl() => GetUrl(x => x.Account.Subscription(null));

    public string MessageAdminUrl(Chapter chapter, Guid messageId)
        => GetUrl(chapter, x => x.GroupAdmin.Message(chapter, messageId).Path);

    public string MessageSiteAdminUrl(Guid messageId) => GetUrl(x => x.SiteAdmin.Message(messageId).Path);

    public string PasswordReset(Chapter? chapter, string token)
        => GetUrl(chapter, x => x.Account.PasswordReset(chapter, token));

    public string RefuseInviteUrl(Chapter chapter, string inviteToken)
        => GetUrl(chapter, x => x.Groups.RefuseInvite(chapter, inviteToken));

    public string SiteAdminGroups() => GetUrl(x => x.SiteAdmin.Groups.Path);

    public string SiteConversationAdminUrl(Guid conversationId)
        => GetUrl(x => x.SiteAdmin.Conversation(conversationId).Path);

    public string SiteConversationUrl(Guid conversationId)
        => GetUrl(x => x.Account.SiteConversation(conversationId));

    public string TopicApprovalUrl() => GetUrl(x => x.SiteAdmin.Topics.Path);

    /// <summary>A URL naming no group, built against the platform this provider was given.</summary>
    private string GetUrl(Func<IOdkRoutes, string> path) => GetUrl(chapter: null, path);

    /// <summary>
    /// A URL about a group, built against the platform that owns it. A null group means the URL is not about
    /// one - the chapter-optional routes take one where the platform has a chapter-scoped page for it - so it
    /// falls back to the platform this provider was given.
    /// </summary>
    private string GetUrl(Chapter? chapter, Func<IOdkRoutes, string> path)
    {
        var platform = chapter?.Platform ?? _platform;
        return $"{_platformProvider.GetBaseUrl(platform)}{path(_odkRoutesFactory.Get(platform))}";
    }
}
