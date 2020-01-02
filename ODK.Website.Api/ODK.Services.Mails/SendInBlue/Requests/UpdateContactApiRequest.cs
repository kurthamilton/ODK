namespace ODK.Services.Emails.SendInBlue.Requests
{
    public class UpdateContactApiRequest
    {
        public UpdateContactAttributesApiRequest Attributes { get; set; }

        public bool EmailBlacklisted { get; set; }
    }
}
