namespace ODK.Core.Countries;

public class Currency : IDatabaseEntity
{
    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    public Guid Id { get; set; }

    public string Symbol { get; set; } = "";
}
