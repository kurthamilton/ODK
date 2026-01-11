using System;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public string Activate(Chapter? chapter, string token)
        => AccountPath(chapter, $"/activate?token={HttpUtility.UrlEncode(token)}");

    public string Conversations(Chapter? chapter) => AccountPath(chapter, "/conversations");

    public string Delete(Chapter? chapter) => AccountPath(chapter, "/delete");

    public string EmailAddressChange(PlatformType platform, Chapter? chapter) =>
        platform == PlatformType.DrunkenKnitwits
            ? AccountPath(chapter, "/email/change")
            : EmailPreferences(chapter);

    public string EmailAddressChangeConfirm(Chapter? chapter, string token)
        => AccountPath(chapter, $"/email/change/confirm?token={HttpUtility.UrlEncode(token)}");

    public string EmailPreferences(Chapter? chapter) => AccountPath(chapter, "/emails");

    public string Groups() => AccountPath(null, "/groups");

    public string Index(Chapter? chapter) => AccountPath(chapter, "/");

    public string Interests() => AccountPath(null, "/interests");

    public string Issue(Guid issueId) => $"{Issues()}/{issueId}";

    public string Issues() => AccountPath(null, $"/issues");

    public string Join(Chapter? chapter) => AccountPath(chapter, "/join");

    public string Location() => AccountPath(null, "/location");

    public string Login(Chapter? chapter) => AccountPath(chapter, "/login");

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

    public string Subscription(PlatformType platform, Chapter? chapter) => AccountPath(chapter, "/subscription");

    private string AccountPath(Chapter? chapter, string path) => GetRoute(chapter, "/account" + path);
}