using System.Collections.Generic;

namespace ODK.Services.Emails.SendInBlue.Requests
{
    public class SendTestEmailCampaignEmailApiRequest
    {
        public IEnumerable<string> EmailTo { get; set; }
    }
}
