using ODK.Core.Members;

namespace ODK.Services.Authentication;

public class PasswordHasherOptions : IHashedPasswordOptions
{
    public required string Algorithm { get; init; }

    public required int Iterations { get; init; }

    public required string Salt { get; init; }
}
