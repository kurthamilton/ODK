namespace ODK.Web.Common.Account.Requests
{
    public class ActivateAccountApiRequest
    {
        public string ActivationToken { get; set; }

        public string Password { get; set; }
    }
}
