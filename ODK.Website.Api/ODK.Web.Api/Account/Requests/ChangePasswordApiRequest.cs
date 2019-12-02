namespace ODK.Web.Api.Account.Requests
{
    public class ChangePasswordApiRequest
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
