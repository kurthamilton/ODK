namespace ODK.Web.Api.Account.Requests
{
    public class ResetPasswordApiRequest
    {
        public string Password { get; set; }

        public string Token { get; set; }
    }
}
