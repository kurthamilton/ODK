namespace ODK.Services.Emails.SendInBlue.Responses
{
    public class EmailCampaignApiResponse
    {
        public int Id { get; set; }

        public EmailCampaignStatisticsApiResponse Statistics { get; set; }
    }
}
