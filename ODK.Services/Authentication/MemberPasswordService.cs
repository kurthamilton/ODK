using ODK.Core.Members;

namespace ODK.Services.Authentication;

public class MemberPasswordService : IMemberPasswordService
{
    private readonly IBreachedPasswordChecker _breachedPasswordChecker;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicy _passwordPolicy;

    public MemberPasswordService(
        IPasswordPolicy passwordPolicy,
        IPasswordHasher passwordHasher,
        IBreachedPasswordChecker breachedPasswordChecker)
    {
        _breachedPasswordChecker = breachedPasswordChecker;
        _passwordHasher = passwordHasher;
        _passwordPolicy = passwordPolicy;
    }

    public MemberPassword Apply(MemberPassword? memberPassword, string password)
    {
        var (hash, options) = _passwordHasher.ComputeHash(password);

        memberPassword ??= new MemberPassword();
        memberPassword.Hash = hash;
        memberPassword.Salt = options.Salt;
        memberPassword.Algorithm = options.Algorithm;
        memberPassword.Iterations = options.Iterations;

        return memberPassword;
    }

    public async Task<ServiceResult> Validate(string password)
    {
        var error = _passwordPolicy.GetValidationError(password);
        if (error != null)
        {
            return ServiceResult.Failure(error);
        }

        if (await _breachedPasswordChecker.IsBreached(password))
        {
            return ServiceResult.Failure(
                "This password has appeared in a known data breach. Please choose a different password.");
        }

        return ServiceResult.Successful();
    }
}
