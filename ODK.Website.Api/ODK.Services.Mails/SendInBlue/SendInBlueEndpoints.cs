namespace ODK.Services.Mails.SendInBlue
{
    public static class SendInBlueEndpoints
    {
        private const string BaseUrl = "https://api.sendinblue.com/v3";

        public const string ContactLists = BaseUrl + "/Contacts/Lists";

        public const string EmailCampaigns = BaseUrl + "/EmailCampaigns";

        public static string EmailCampaign(int id)
        {
            return $"{EmailCampaigns}/{id}";
        }

        public static string EmailCampaignSend(int id)
        {
            return $"{EmailCampaign(id)}/SendNow";
        }
    }
}
