using System.Collections.Generic;

namespace ODK.Services.Emails.SendInBlue.Requests
{
    public class CreateContactApiRequest
    {
        public CreateContactAttributesApiRequest Attributes { get; set; }

        public string Email { get; set; }

        public bool EmailBlacklisted { get; set; }

        public IEnumerable<int> ListIds { get; set; }
    }
}
