using System;

namespace ODK.Core.Events
{
    public class Event
    {
        public Event(Guid id, Guid chapterId, string createdBy, string name, DateTime date, Guid venueId, string time, 
            string imageUrl, string description, bool isPublic)
        {
            ChapterId = chapterId;
            CreatedBy = createdBy;
            Date = date;
            Description = description;
            Id = id;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            Name = name;
            Time = time;
            VenueId = venueId;
        }

        public Guid ChapterId { get; }

        public string CreatedBy { get; }

        public DateTime Date { get; private set; }

        public string Description { get; private set; }

        public Guid Id { get; }

        public string ImageUrl { get; private set; }

        public bool IsPublic { get; private set; }

        public string Name { get; private set; }        

        public double? TicketCost => throw new NotImplementedException();

        public int? TicketCount => throw new NotImplementedException();

        public DateTime? TicketDeadline => throw new NotImplementedException();

        public string Time { get; private set; }

        public Guid VenueId { get; private set; }

        public void Update(DateTime date, string description, string imageUrl, bool isPublic,
            string name, string time, Guid venueId)
        {
            Date = date;
            Description = description;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            Name = name;
            Time = time;
            VenueId = venueId;
        }
    }
}
