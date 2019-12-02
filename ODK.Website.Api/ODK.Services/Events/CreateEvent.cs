using System;

namespace ODK.Services.Events
{
    public class CreateEvent
    {
        public string Address { get; set; }

        public Guid ChapterId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string MapQuery { get; set; }

        public string Name { get; set; }

        public bool IsPublic { get; set; }

        public string Time { get; set; }
    }
}
