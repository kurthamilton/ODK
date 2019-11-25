using System;

namespace ODK.Core.Events
{
    public class Event
    {
        public Event(Guid id, Guid chapterId, string name, DateTime date, string location, string time, 
            string imageUrl, string address, string mapQuery, string description, bool isPublic)
        {
            Address = address;
            ChapterId = chapterId;
            Date = date;
            Description = description;
            Id = id;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            Location = location;
            MapQuery = mapQuery;
            Name = name;
            Time = time;
        }

        public string Address { get; }

        public Guid ChapterId { get; }

        public DateTime Date { get; }

        public string Description { get; }

        public Guid Id { get; }

        public string ImageUrl { get; }

        public DateTime? InviteSentDate { get; }

        public bool IsPublic { get; }

        public string Location { get; }

        public string MapQuery { get; }

        public string Name { get; }        

        public double? TicketCost => throw new NotImplementedException();

        public int? TicketCount => throw new NotImplementedException();

        public DateTime? TicketDeadline => throw new NotImplementedException();

        public string Time { get; }
    }
}
