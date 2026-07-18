namespace ODK.Services.Authentication;

public interface IPasswordPolicy
{
    int MinLength { get; }

    /// <summary>
    /// Returns a validation error message, or null when the password satisfies the length policy.
    /// </summary>
    string? GetValidationError(string? password);
}
