namespace ODK.Services;

public class ServiceResult
{
    private ServiceResult(bool success, string message = "")
    {
        Message = message;
        Success = success;
    }

    public string Message { get; }

    public bool Success { get; }

    public static ServiceResult Failure(string message)
    {
        return new ServiceResult(false, message);
    }

    public static ServiceResult Successful(string? message = null)
    {
        return new ServiceResult(true, message ?? "");
    }
}
