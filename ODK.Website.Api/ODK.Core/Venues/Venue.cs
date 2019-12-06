using System;

namespace ODK.Core.Venues
{
    public class Venue
    {
        public Venue(Guid id, Guid chapterId, string name, string address, string mapQuery)
        {
            Address = address;
            ChapterId = chapterId;
            Id = id;
            MapQuery = mapQuery;
            Name = name;
        }

        public string Address { get; }

        public Guid ChapterId { get; }

        public Guid Id { get; }

        public string MapQuery { get; }

        public string Name { get; }
    }
}
