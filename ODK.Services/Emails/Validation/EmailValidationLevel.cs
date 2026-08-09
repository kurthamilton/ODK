namespace ODK.Services.Emails.Validation;

public enum EmailValidationLevel
{
    None = 0,

    /// <summary>
    /// Format only, no external call. For anonymous public forms, where the volume is not ours to control
    /// and a third-party quota could be drained by anyone who found the form.
    /// </summary>
    Soft = 1,

    /// <summary>
    /// Format, then a deliverability check with the configured verifier. For addresses the app is going to
    /// rely on - an account it will send activation to, or a referral it will email.
    /// </summary>
    Full = 2
}
