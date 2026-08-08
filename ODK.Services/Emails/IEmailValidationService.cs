namespace ODK.Services.Emails;

/// <summary>
/// The single place an email address is checked before the app accepts or sends to it.
/// </summary>
public interface IEmailValidationService
{
    Task<ServiceResult> Validate(string emailAddress);
}
