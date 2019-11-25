namespace ODK.Web.Api.Account
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
