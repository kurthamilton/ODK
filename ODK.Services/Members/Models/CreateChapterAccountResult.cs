namespace ODK.Services.Members.Models;

/// <summary>
/// The outcome of creating a chapter account, which on Drunken Knitwits is also joining the group.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of string: the payload is a token, and only its
/// property name can say which kind. A caller reading <c>Value</c> has to be told what it holds; one reading
/// <see cref="ActivationToken"/> is told by the code.
/// </remarks>
public class CreateChapterAccountResult : ServiceResult
{
    private CreateChapterAccountResult(bool success, string? message = null, string? activationToken = null)
        : base(success, message)
    {
        ActivationToken = activationToken;
    }

    /// <summary>
    /// Set when the sign-up itself proved the address - it arrived with an invitation sent there - so the account
    /// can be activated straight away and no activation email was sent. Null on every other outcome, including
    /// the successful ones, where an activation email carries the token instead.
    /// </summary>
    public string? ActivationToken { get; }

    public new static CreateChapterAccountResult Failure(string message) => new(false, message);

    /// <summary>
    /// Carries a failure raised elsewhere - validation, image processing - through unchanged.
    /// </summary>
    public static CreateChapterAccountResult FromResult(ServiceResult result) => new(result.Success, result.Message);

    /// <summary>
    /// The account was created and an activation email sent, so there is nothing for the caller to do but tell
    /// them to expect it.
    /// </summary>
    public new static CreateChapterAccountResult Successful(string? message = null) => new(true, message);

    /// <summary>
    /// The account was created and needs no activation email, so the caller can send them straight to setting a
    /// password with <paramref name="activationToken"/>.
    /// </summary>
    public static CreateChapterAccountResult SuccessfulReadyToActivate(string activationToken)
        => new(true, activationToken: activationToken);
}
