using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public string Conversations(Chapter? chapter) => AccountPath(chapter, "/conversations");
    public string Delete(Chapter? chapter) => AccountPath(chapter, "/delete");
    public string EmailAddressChange(Chapter? chapter) => AccountPath(chapter, "/email/change");
    public string EmailPreferences(Chapter? chapter) => AccountPath(chapter, "/emails");
    public string Join(Chapter? chapter) => AccountPath(chapter, "/join");
    public string Location() => AccountPath(null, "/location");
    public string Login(Chapter? chapter) => AccountPath(chapter, "/login");
    public string PasswordChange(Chapter? chapter) => AccountPath(chapter, "/password/change");
    public string PersonalDetails(Chapter? chapter) => AccountPath(chapter, "");
    public string Picture(Chapter? chapter) => AccountPath(chapter, "/picture");
    public string PictureRotate(Chapter? chapter) => AccountPath(chapter, "/picture/rotate");
    public string PictureUpload(Chapter? chapter) => AccountPath(chapter, "/picture/change");
    public string Profile(Chapter? chapter) => AccountPath(chapter, "/profile");
    public string Subscription(Chapter? chapter) => AccountPath(chapter, "/subscription");

    private string AccountPath(Chapter? chapter, string path) => GetRoute(chapter, "/account" + path);
}
