using System;

namespace ODK.Services.Events
{
    public class CreateEvent
    {
        public Guid ChapterId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public bool IsPublic { get; set; }

        public string Name { get; set; }

        public string Time { get; set; }

        public Guid VenueId { get; set; }
    }
}
