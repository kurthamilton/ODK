using System;
using ODK.Core.Members;

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

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public Guid Id { get; }

        public string ImageUrl { get; set; }

        public bool IsPublic { get; set; }

        public string Name { get; set; }

        public double? TicketCost => throw new NotImplementedException();

        public int? TicketCount => throw new NotImplementedException();

        public DateTime? TicketDeadline => throw new NotImplementedException();

        public string Time { get; set; }

        public Guid VenueId { get; set; }

        public bool IsAuthorized(Member member)
        {
            return IsPublic || member?.ChapterId == ChapterId;
        }
    }
}
