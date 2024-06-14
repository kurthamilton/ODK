namespace ODK.Services.Venues;

public class CreateVenue
{
    public string Address { get; set; } = "";

    public Guid ChapterId { get; set; }

    public string MapQuery { get; set; } = "";

    public string Name { get; set; } = "";
}
