namespace ODK.Services.Emails.SendInBlue
{
    public static class SendInBlueEndpoints
    {
        private const string BaseUrl = "https://api.sendinblue.com/v3";

        public const string ContactLists = Contacts + "/Lists";

        public const string Contacts = BaseUrl + "/Contacts";

        public const string EmailCampaigns = BaseUrl + "/EmailCampaigns";

        public static string Contact(string email)
        {
            return $"{Contacts}/{email}";
        }

        public static string ContactListContacts(int listId)
        {
            return $"{Contacts}/Lists/{listId}/Contacts?limit=500";
        }

        public static string EmailCampaign(int id)
        {
            return $"{EmailCampaigns}/{id}";
        }

        public static string EmailCampaignSend(int id)
        {
            return $"{EmailCampaign(id)}/SendNow";
        }

        public static string EmailCampaignSendTest(int id)
        {
            return $"{EmailCampaign(id)}/SendTest";
        }
    }
}
