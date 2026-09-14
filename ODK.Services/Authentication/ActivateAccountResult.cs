using ODK.Core.Members;

namespace ODK.Services.Authentication;

/// <summary>
/// The outcome of following an activation link, and what the sign-up that issued it was for. The caller
/// needs the second to decide where to send the member next, which is the whole reason the intent is
/// carried on the token.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/>, so the value reads as an intent at the
/// use site rather than as <c>Value</c>.
/// </remarks>
public class ActivateAccountResult : ServiceResult
{
    private ActivateAccountResult(bool success, string? message = null, SignUpIntentType? intent = null)
        : base(success, message)
    {
        Intent = intent;
    }

    /// <summary>What the sign-up was for, where it stated anything. Null on a failure.</summary>
    public SignUpIntentType? Intent { get; }

    public new static ActivateAccountResult Failure(string message) => new(false, message);

    /// <summary>Carries a failure raised by the machine through unchanged.</summary>
    public static ActivateAccountResult FromResult(ServiceResult result) => new(result.Success, result.Message);

    /// <summary>The account was activated, and the sign-up that issued its token stated this.</summary>
    public static ActivateAccountResult Activated(SignUpIntentType? intent) => new(true, intent: intent);
}
