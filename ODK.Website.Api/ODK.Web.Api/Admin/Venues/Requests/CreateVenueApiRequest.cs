using System;

namespace ODK.Web.Api.Admin.Venues.Requests
{
    public class CreateVenueApiRequest
    {
        public string Address { get; set; }

        public Guid ChapterId { get; set; }

        public string MapQuery { get; set; }

        public string Name { get; set; }
    }
}
