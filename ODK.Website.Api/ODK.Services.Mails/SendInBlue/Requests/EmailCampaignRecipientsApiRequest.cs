using System.Collections.Generic;

namespace ODK.Services.Emails.SendInBlue.Requests
{
    public class EmailCampaignRecipientsApiRequest
    {
        public IEnumerable<int> ListIds { get; set; }
    }
}
