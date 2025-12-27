namespace ODK.Core.Logging;

public class Error : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public string ExceptionMessage { get; set; } = string.Empty;

    public string ExceptionType { get; set; } = string.Empty;

    public Guid Id { get; set; } 

    public static Error FromException(Exception exception)
    {
        return new Error
        {
            CreatedUtc = DateTime.UtcNow,
            ExceptionMessage = exception.Message,
            ExceptionType = exception.GetType().Name
        };
    }
}
