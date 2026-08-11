namespace ODK.Services.Emails;

/// <summary>
/// A set of values an email template can interpolate. Implement this per email rather than passing a
/// loose dictionary around, so the parameters an email supports are declared somewhere a caller can
/// read - a missing or misspelt key otherwise surfaces as literal braces in a sent email.
/// </summary>
public interface IEmailParameters
{
    IReadOnlyDictionary<string, string> ToDictionary();
}
