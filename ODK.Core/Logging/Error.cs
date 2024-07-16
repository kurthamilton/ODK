namespace ODK.Core.Logging;

public class Error : IDatabaseEntity
{
    public DateTime CreatedDate { get; set; }

    public string ExceptionMessage { get; set; } = "";

    public string ExceptionType { get; set; } = "";

    public Guid Id { get; set; } 

    public static Error FromException(Exception exception)
    {
        return new Error
        {
            CreatedDate = DateTime.UtcNow,
            ExceptionMessage = exception.Message,
            ExceptionType = exception.GetType().Name
        };
    }
}
