using System;
using ODK.Core.Members;

namespace ODK.Web.Api.Chapters.Responses
{
    public class ChapterSubscriptionApiResponse
    {
        public double Amount { get; set; }

        public Guid ChapterId { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public SubscriptionType SubscriptionType { get; set; }

        public string Title { get; set; }
    }
}
