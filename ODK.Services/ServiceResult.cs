namespace ODK.Services;

public class ServiceResult<T> : ServiceResult
{
    private ServiceResult(bool success, string message = "", T? value = default)
        : base(success, message)
    {
        Value = value;
    }

    public T? Value { get; }

    public static new ServiceResult<T> Failure(string message) 
        => new ServiceResult<T>(false, message);

    public static ServiceResult<T> Successful(T value, string? message = null) 
        => new ServiceResult<T>(true, message ?? "", value);
}

public class ServiceResult
{
    protected ServiceResult(bool success, string message = "")
    {
        Message = message;
        Success = success;
    }

    public string Message { get; }

    public bool Success { get; }

    public static ServiceResult Failure(string message) => new ServiceResult(false, message);

    public static ServiceResult Successful(string? message = null) => new ServiceResult(true, message ?? "");
}
