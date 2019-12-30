using System;

namespace ODK.Web.Api.Venues.Responses
{
    public class VenueApiResponse
    {
        public string Address { get; set; }

        public Guid ChapterId { get; set; }

        public Guid Id { get; set; }

        public string MapQuery { get; set; }

        public string Name { get; set; }
    }
}
