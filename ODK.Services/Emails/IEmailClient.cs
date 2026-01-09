namespace ODK.Services.Emails;

public interface IEmailClient
{
    Task<SendEmailResult> SendEmail(EmailClientEmail email);
}
