namespace ODK.Core.Logging;

public class ErrorProperty
{
    public Guid ErrorId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}
