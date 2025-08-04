namespace ODK.Core.Exceptions;

public class OdkNotFoundException : Exception
{
    public OdkNotFoundException()
    {
    }

    public OdkNotFoundException(string message)
        : base(message)
    {
    }
}
