using ODK.Core.Members;

namespace ODK.Web.Api.Admin.Chapters.Requests
{
    public class CreateChapterSubscriptionApiRequest
    {
        public double Amount { get; set; }

        public string Description { get; set; }

        public int Months { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public SubscriptionType Type { get; set; }
    }
}
