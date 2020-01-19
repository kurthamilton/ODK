namespace ODK.Web.Common.Account.Requests
{
    public class ChangePasswordApiRequest
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
