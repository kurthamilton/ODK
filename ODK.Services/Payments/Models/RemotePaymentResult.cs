namespace ODK.Services.Payments.Models;

public class RemotePaymentResult
{
    public string? Id { get; init; }

    public string? Message { get; init; }

    public required bool Success { get; init; }

    public static RemotePaymentResult Successful(string id)
    {
        return new RemotePaymentResult
        {
            Id = id,
            Success = true
        };
    }

    public static RemotePaymentResult Failure(string? message = null)
    {
        return new RemotePaymentResult
        {
            Message = message,
            Success = false
        };
    }
}
