using ODK.Core.Features;

namespace ODK.Services;

public class ServiceResult<T> : ServiceResult
{
    private ServiceResult(bool success, string? message = null, T? value = default)
        : base(success, message)
    {
        Value = value;
    }

    public T? Value { get; }

    public new static ServiceResult<T> Failure(string message)
        => new ServiceResult<T>(false, message);

    public static ServiceResult<T> Failure(string message, T? value)
        => new ServiceResult<T>(false, message, value);

    public static ServiceResult<T> FromResult(ServiceResult result, T? value)
        => new ServiceResult<T>(result.Success, result.Message, value);

    public static ServiceResult<T> Successful(T value, string? message = null)
        => new ServiceResult<T>(true, message, value);
}

public class ServiceResult
{
    protected ServiceResult(bool success, string? message = null)
    {
        Messages = message != null ? [message] : [];
        Success = success;
    }

    protected ServiceResult(IEnumerable<string> messages)
    {
        Messages = messages.ToArray();
        Success = Messages.Count == 0;
    }

    public string? Message => Messages.FirstOrDefault();

    public IReadOnlyCollection<string> Messages { get; } = [];

    public bool Success { get; }

    public static ServiceResult Failure(string? message) => new(false, message);

    public static ServiceResult Failure(IEnumerable<string> messages) => new(messages);

    public static ServiceResult Successful(string? message = null) => new(true, message);

    public static ServiceResult Unauthorized(SiteFeatureType feature) => Failure("You do not have access to this feature");
}