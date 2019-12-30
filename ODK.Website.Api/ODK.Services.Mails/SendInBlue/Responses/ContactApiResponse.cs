namespace ODK.Services.Mails.SendInBlue.Responses
{
    public class ContactApiResponse
    {
        public ContactAttributesApiResponse Attributes { get; set; }

        public string Email { get; set; }

        public bool EmailBlacklisted { get; set; }

        public int Id { get; set; }
    }
}
