namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public string Delete(string? chapterName) => GetRoute(chapterName, "Delete");
    public string EmailAddressChange(string? chapterName) => GetRoute(chapterName, "Email/Change");
    public string EmailPreferences(string? chapterName) => GetRoute(chapterName, "Emails");
    public string Join(string? chapterName) => GetRoute(chapterName, "Join");
    public string PasswordChange(string? chapterName) => GetRoute(chapterName, "Password/Change");
    public string PictureRotate(string? chapterName) => GetRoute(chapterName, "Picture/Rotate");
    public string PictureUpload(string? chapterName) => GetRoute(chapterName, "Picture/Change");
    public string Subscription(string? chapterName) => GetRoute(chapterName, "Subscription");
}
