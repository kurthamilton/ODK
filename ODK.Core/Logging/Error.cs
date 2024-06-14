namespace ODK.Core.Logging;

public class Error
{
    public Error(Guid id, DateTime createdDate, string exceptionType, string exceptionMessage)
    {
        CreatedDate = createdDate;
        ExceptionMessage = exceptionMessage;
        ExceptionType = exceptionType;
        Id = id;
    }

    public DateTime CreatedDate { get; }

    public string ExceptionMessage { get; }

    public string ExceptionType { get; }

    public Guid Id { get; } 
}
