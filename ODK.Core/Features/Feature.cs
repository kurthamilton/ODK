namespace ODK.Core.Features;

public class Feature : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public string Description { get; set; } = "";

    public Guid Id { get; set; }

    public string Name { get; set; } = "";
}
