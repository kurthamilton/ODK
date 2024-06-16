namespace ODK.Core.Venues;

public class Venue : IVersioned
{
    public Venue(Guid id, Guid chapterId, string name, string? address, string? mapQuery, long version)
    {
        Address = address;
        ChapterId = chapterId;
        Id = id;
        MapQuery = mapQuery;
        Name = name;
        Version = version;
    }

    public string? Address { get; private set; }

    public Guid ChapterId { get; }

    public Guid Id { get; }

    public string? MapQuery { get; private set; }

    public string Name { get; private set; }

    public long Version { get; }

    public void Update(string name, string? address, string? mapQuery)
    {
        Address = address;
        MapQuery = mapQuery;
        Name = name;
    }
}
