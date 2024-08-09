namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public string Delete(string? chapterName) => AccountPath(chapterName, "delete");
    public string EmailAddressChange(string? chapterName) => AccountPath(chapterName, "email/change");
    public string EmailPreferences(string? chapterName) => AccountPath(chapterName, "emails");
    public string Join(string? chapterName) => AccountPath(chapterName, "join");
    public string Location(string? chapterName) => AccountPath(chapterName, "location");
    public string PasswordChange(string? chapterName) => AccountPath(chapterName, "password/change");
    public string PersonalDetails(string? chapterName) => AccountPath(chapterName, "");
    public string Picture(string? chapterName) => AccountPath(chapterName, "picture");
    public string PictureRotate(string? chapterName) => AccountPath(chapterName, "picture/rotate");
    public string PictureUpload(string? chapterName) => AccountPath(chapterName, "picture/change");
    public string Profile(string chapterName) => AccountPath(chapterName, "profile");
    public string Subscription(string? chapterName) => AccountPath(chapterName, "subscription");

    private string AccountPath(string? chapterName, string path)
        => GetRoute(chapterName, "account" + (path != "" ? $"/{path}" : ""));
}
