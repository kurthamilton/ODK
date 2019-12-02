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

        public string Address { get; private set; }

        public Guid ChapterId { get; }

        public DateTime Date { get; private set; }

        public string Description { get; private set; }

        public Guid Id { get; }

        public string ImageUrl { get; private set; }

        public DateTime? InviteSentDate { get; }

        public bool IsPublic { get; private set; }

        public string Location { get; private set; }

        public string MapQuery { get; private set; }

        public string Name { get; private set; }        

        public double? TicketCost => throw new NotImplementedException();

        public int? TicketCount => throw new NotImplementedException();

        public DateTime? TicketDeadline => throw new NotImplementedException();

        public string Time { get; private set; }

        public void Update(string address, DateTime date, string description, string imageUrl, bool isPublic,
            string location, string mapQuery, string name, string time)
        {
            Address = address;
            Date = date;
            Description = description;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            Location = location;
            MapQuery = mapQuery;
            Name = name;
            Time = time;
        }
    }
}
