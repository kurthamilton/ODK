namespace ODK.Services.Mails.SendInBlue.Requests
{
    public class CreateEmailCampaignApiRequest
    {
        public string HtmlContent { get; set; }

        public string Name { get; set; }

        public EmailCampaignRecipientsApiRequest Recipients { get; set; }

        public string ReplyTo { get; set; }

        public EmailCampaignSenderApiRequest Sender { get; set; }

        public string Subject { get; set; }
    }
}
