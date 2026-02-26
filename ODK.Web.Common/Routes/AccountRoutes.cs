using System;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public AccountRoutes(PlatformType platform)
        : base(platform)
    {
    }

    public string Activate(Chapter? chapter, string token)
        => AccountPath(chapter, $"/activate?token={HttpUtility.UrlEncode(token)}");

    public string Create() => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => AccountPath(null, "/create")
    };

    public string Conversations(Chapter? chapter) => AccountPath(chapter, "/conversations");

    public string Delete(Chapter? chapter) => AccountPath(chapter, "/delete");

    public string EmailAddressChange(Chapter? chapter) =>
        Platform == PlatformType.DrunkenKnitwits
            ? AccountPath(chapter, "/email/change")
            : EmailPreferences(chapter);

    public string EmailAddressChangeConfirm(Chapter? chapter, string token)
        => AccountPath(chapter, $"/email/change/confirm?token={HttpUtility.UrlEncode(token)}");

    public string EmailPreferences(Chapter? chapter) => AccountPath(chapter, "/emails");

    public string ForgottenPassword(Chapter? chapter) => AccountPath(chapter, "/password/forgotten");

    public string Groups() => AccountPath(null, "/groups");

    public string Index(Chapter? chapter) => AccountPath(chapter, "/");

    public string Interests() => AccountPath(null, "/interests");

    public string Issue(Guid issueId) => $"{Issues()}/{issueId}";

    public string Issues() => AccountPath(null, $"/issues");

    public string Join(Chapter? chapter) => AccountPath(chapter, "/join");

    public string Location() => AccountPath(null, "/location");

    public string LocationDefaults(string latPlaceholder, string longPlaceholder)
        => AccountPath(null, $"/location/defaults?lat={latPlaceholder}&long={longPlaceholder}");

    public string Login(Chapter? chapter) => AccountPath(Platform switch
    {
        PlatformType.DrunkenKnitwits => chapter,
        _ => null
    }, "/login");

    public string Logout(Chapter? chapter) => AccountPath(Platform switch
    {
        PlatformType.DrunkenKnitwits => chapter,
        _ => null
    }, "/logout");

    public string Notifications(Chapter? chapter) => AccountPath(chapter, "/notifications");

    public string PasswordChange(Chapter? chapter) => AccountPath(chapter, "/password/change");

    public string PasswordReset(Chapter? chapter, string token)
        => AccountPath(chapter, $"/password/reset?token={HttpUtility.UrlEncode(token)}");

    public string Payments(Chapter? chapter) => AccountPath(chapter, "/payments");

    public string PersonalDetails(Chapter? chapter) => AccountPath(chapter, string.Empty);

    public string Picture(Chapter? chapter) => AccountPath(chapter, "/picture");

    public string PictureRotate(Chapter? chapter) => AccountPath(chapter, "/picture/rotate");

    public string PictureUpload(Chapter? chapter) => AccountPath(chapter, "/picture/change");

    public string Profile(Chapter? chapter) => AccountPath(chapter, "/profile");

    public string Subscription(Chapter? chapter) => AccountPath(chapter, "/subscription");

    public string SiteSubscriptionCheckout(Guid priceId)
        => AccountPath(chapter: null, $"/subscription/{priceId}/checkout");

    public string SiteSubscriptionConfirm()
        => $"{Subscription(chapter: null)}/confirm?sessionId={{sessionId}}";

    private string AccountPath(Chapter? chapter, string path)
        => GetRoute(chapter, "/account" + path);
}