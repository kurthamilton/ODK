namespace ODK.Web.Common.Account.Requests
{
    public class ResetPasswordApiRequest
    {
        public string Password { get; set; }

        public string Token { get; set; }
    }
}
