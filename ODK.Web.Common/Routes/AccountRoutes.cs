using System;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public AccountRoutes(PlatformType platform)
        : base(platform)
    {
    }

    public string Activate(Chapter? chapter, string token)
        => AccountPath(
            Platform == PlatformType.DrunkenKnitwits ? chapter : null,
            $"/activate?token={Uri.EscapeDataString(token)}");

    public string Create() => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => AccountPath(null, "/create")
    };

    /// <summary>
    /// The sign-up page, told what the member is signing up in order to do, so the journey that brought
    /// them here survives an activation email and a sign-in.
    /// </summary>
    public string Create(SignUpIntentType intent) => intent == SignUpIntentType.None
        ? Create()
        : $"{Create()}?intent={intent}";

    public string Conversation(Chapter? chapter, Guid id) => $"{Conversations(chapter)}/{id}";

    public string Conversations(Chapter? chapter) => Conversations(chapter, archived: false);

    public string Conversations(Chapter? chapter, bool archived)
        => AccountPath(chapter, $"/conversations{(archived ? "?archived=true" : null)}");

    /// <summary>
    /// A member's thread with the site's admins. Never chapter-scoped, unlike <see cref="Conversation"/>.
    /// </summary>
    /// <remarks>
    /// Drunken Knitwits has no site account for this to sit under - an account there belongs to a chapter -
    /// so it gets a page of its own rather than a tab in an account area that does not exist. Group Squirrel
    /// keeps it beside the group conversations it is tabbed with.
    /// </remarks>
    public string SiteConversation(Guid id) => $"{SiteConversations()}/{id}";

    public string SiteConversations() => SiteConversations(archived: false);

    public string SiteConversations(bool archived)
    {
        var query = archived ? "?archived=true" : null;

        return Platform switch
        {
            PlatformType.DrunkenKnitwits => $"/conversations{query}",
            _ => AccountPath(null, $"/site-conversations{query}")
        };
    }

    public string Delete(Chapter? chapter) => AccountPath(chapter, "/delete");

    public string EmailAddressChange(Chapter? chapter) =>
        Platform == PlatformType.DrunkenKnitwits
            ? AccountPath(chapter, "/email/change")
            : EmailPreferences(chapter);

    public string EmailAddressChangeConfirm(Chapter? chapter, string token)
        => AccountPath(chapter, $"/email/change/confirm?token={Uri.EscapeDataString(token)}");

    public string EmailPreferences(Chapter? chapter) => AccountPath(chapter, "/emails");

    public string ForgottenPassword(Chapter? chapter) => AccountPath(chapter, "/password/forgotten");

    public string Groups() => AccountPath(null, "/groups");

    public string Index(Chapter? chapter) => AccountPath(chapter, "");

    public string Interests() => AccountPath(null, "/interests");

    public string Join(Chapter? chapter) => AccountPath(chapter, "/join");

    public string Location() => AccountPath(null, "/location");

    public string LocationDefaults(string latPlaceholder, string longPlaceholder)
        => AccountPath(null, $"/location/defaults?lat={latPlaceholder}&long={longPlaceholder}");

    public string Login(Chapter? chapter) => AccountPath(Platform switch
    {
        PlatformType.DrunkenKnitwits => chapter,
        _ => null
    }, "/login");

    public string Login(Chapter? chapter, string? returnUrl)
    {
        var url = Login(chapter);
        return string.IsNullOrEmpty(returnUrl)
            ? url
            : $"{url}?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    public string Logout(Chapter? chapter) => AccountPath(Platform switch
    {
        PlatformType.DrunkenKnitwits => chapter,
        _ => null
    }, "/logout");

    /// <summary>
    /// Signs the current member out of a cookie holding several accounts, leaving the rest signed in.
    /// Not chapter-scoped on either platform - it acts on the cookie, not on a group.
    /// </summary>
    public string LogoutAccount() => "/account/accounts/logout";

    public string Notifications(Chapter? chapter) => AccountPath(chapter, "/notifications");

    public string PasswordChange(Chapter? chapter) => AccountPath(chapter, "/password/change");

    public string PasswordReset(Chapter? chapter, string token)
        => AccountPath(chapter, $"/password/reset?token={Uri.EscapeDataString(token)}");

    public string Payments(Chapter? chapter) => AccountPath(chapter, "/payments");

    public string Pending(Chapter? chapter) => AccountPath(
        Platform == PlatformType.DrunkenKnitwits ? chapter : null,
        "/pending");

    /// <summary>
    /// The page a sign-up lands on while its activation email is in flight, told what the member was
    /// signing up to do so it can say what is still to come.
    /// </summary>
    public string Pending(Chapter? chapter, SignUpIntentType? intent) => intent == null
        ? Pending(chapter)
        : $"{Pending(chapter)}?intent={intent}";

    public string PersonalDetails(Chapter? chapter) => AccountPath(chapter, string.Empty);

    public string Picture(Chapter? chapter) => AccountPath(chapter, "/picture");

    public string PictureRotate(Chapter? chapter) => AccountPath(chapter, "/picture/rotate");

    public string PictureUpload(Chapter? chapter) => AccountPath(chapter, "/picture/change");

    public string Profile(Chapter? chapter) => AccountPath(chapter, "/profile");

    public string Subscription(Chapter? chapter) => AccountPath(
        Platform == PlatformType.DrunkenKnitwits ? chapter : null,
        "/subscription");

    public string SiteSubscriptionCheckout(Guid priceId)
        => AccountPath(chapter: null, $"/subscription/{priceId}/checkout");

    public string SiteSubscriptionConfirm()
        => $"{Subscription(chapter: null)}/confirm?sessionId={{sessionId}}";

    /// <summary>
    /// Acts as one of the other accounts on the cookie. Not chapter-scoped on either platform - it acts on
    /// the cookie, not on a group.
    /// </summary>
    public string SwitchAccount() => "/account/accounts/switch";

    private string AccountPath(Chapter? chapter, string path)
        => GetRoute(chapter, "/account" + path);
}