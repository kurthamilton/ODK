namespace ODK.Services.Emails;

public class SendEmailResult : ServiceResult
{
    public SendEmailResult(bool success, string message = "") 
        : base(success, message)
    {
    }

    public required string? ExternalId { get; init; }
}
