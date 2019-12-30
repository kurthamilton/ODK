using System.Collections.Generic;

namespace ODK.Services.Mails.SendInBlue.Requests
{
    public class SendTestEmailCampaignEmailApiRequest
    {
        public IEnumerable<string> EmailTo { get; set; }
    }
}
