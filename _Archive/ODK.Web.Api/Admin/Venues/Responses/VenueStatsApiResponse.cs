using System;

namespace ODK.Web.Api.Admin.Venues.Responses
{
    public class VenueStatsApiResponse
    {
        public int EventCount { get; set; }

        public DateTime LastEventDate { get; set; }

        public Guid VenueId { get; set; }
    }
}
