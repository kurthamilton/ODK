namespace ODK.Web.Common.Account
{
    public class LoginHandlerSettings
    {
        public LoginHandlerSettings(int cookieLifetimeDays)
        {
            CookieLifetimeDays = cookieLifetimeDays;
        }

        public int CookieLifetimeDays { get; }
    }
}
