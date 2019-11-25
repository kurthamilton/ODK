namespace ODK.Web.Api.Config
{
    public class AuthSettings
    {
        public int AccessTokenLifetimeMinutes { get; set; }

        public string Key { get; set; }

        public int RefreshTokenLifetimeDays { get; set; }
    }
}
