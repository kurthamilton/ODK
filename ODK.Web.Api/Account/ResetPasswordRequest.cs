namespace ODK.Web.Api.Account
{
    public class ResetPasswordRequest
    {
        public string Password { get; set; }

        public string Token { get; set; }
    }
}
