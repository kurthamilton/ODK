using System;

namespace ODK.Web.Api.Events.Responses
{
    public class EventApiResponse
    {
        public Guid ChapterId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public string ImageUrl { get; set; }

        public bool? IsPublic { get; set; }

        public string Name { get; set; }

        public string Time { get; set; }

        public Guid VenueId { get; set; }
    }
}
