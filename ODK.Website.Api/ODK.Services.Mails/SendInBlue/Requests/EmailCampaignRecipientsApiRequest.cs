using System.Collections.Generic;

namespace ODK.Services.Mails.SendInBlue.Requests
{
    public class EmailCampaignRecipientsApiRequest
    {
        public IEnumerable<int> ListIds { get; set; }
    }
}
