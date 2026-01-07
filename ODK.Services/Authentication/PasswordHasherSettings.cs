namespace ODK.Services.Authentication;

public class PasswordHasherSettings
{
    public required string Algorithm { get; init; }

    public required int Iterations { get; init; }
}
