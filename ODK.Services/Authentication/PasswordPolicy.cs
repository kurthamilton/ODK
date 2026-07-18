namespace ODK.Services.Authentication;

public class PasswordPolicy : IPasswordPolicy
{
    private readonly PasswordPolicySettings _settings;

    public PasswordPolicy(PasswordPolicySettings settings)
    {
        _settings = settings;
    }

    public int MinLength => _settings.MinLength;

    public string? GetValidationError(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password cannot be blank";
        }

        if (password.Length < _settings.MinLength)
        {
            return $"Password must be at least {_settings.MinLength} characters";
        }

        return null;
    }
}
