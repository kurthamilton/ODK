namespace ODK.Web.Common.Routes;

public class AccountRoutes : RoutesBase
{
    public string Delete(string? chapterName) => AccountPath(chapterName, "Delete");
    public string EmailAddressChange(string? chapterName) => AccountPath(chapterName, "Email/Change");
    public string EmailPreferences(string? chapterName) => AccountPath(chapterName, "Emails");
    public string Join(string? chapterName) => AccountPath(chapterName, "Join");
    public string PasswordChange(string? chapterName) => AccountPath(chapterName, "Password/Change");
    public string PersonalDetails(string? chapterName) => AccountPath(chapterName, "");
    public string Picture(string? chapterName) => AccountPath(chapterName, "Picture");
    public string PictureRotate(string? chapterName) => AccountPath(chapterName, "Picture/Rotate");
    public string PictureUpload(string? chapterName) => AccountPath(chapterName, "Picture/Change");
    public string Profile(string chapterName) => AccountPath(chapterName, "Profile");
    public string Subscription(string? chapterName) => AccountPath(chapterName, "Subscription");

    private string AccountPath(string? chapterName, string path)
        => GetRoute(chapterName, "Account" + (path != "" ? $"/{path}" : ""));
}
