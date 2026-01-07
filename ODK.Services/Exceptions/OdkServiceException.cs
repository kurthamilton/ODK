namespace ODK.Services.Exceptions;

public class OdkServiceException : Exception
{
    public OdkServiceException(string message)
        : this(new[] { message })
    {
    }

    public OdkServiceException(IEnumerable<string> messages)
    {
        Messages = messages.ToArray();
    }

    public IReadOnlyCollection<string> Messages { get; }
}
