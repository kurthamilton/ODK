namespace ODK.Services.Emails;

public interface IMailProvider
{
    Task<ServiceResult> SendEmail(SendEmailOptions options);
}
