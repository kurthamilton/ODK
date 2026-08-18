using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Members;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Workflows;

namespace ODK.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAccountContextFactory _accountContextFactory;
    private readonly StateMachineRunner<AccountState, AccountTrigger, AccountContext> _accountWorkflow;
    private readonly IEmailValidationService _emailValidationService;
    private readonly Lazy<IHashedPassword> _dummyPassword;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberPasswordService _memberPasswordService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthenticationServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationService(
        AuthenticationServiceSettings settings,
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IPasswordHasher passwordHasher,
        IMemberPasswordService memberPasswordService,
        IEmailValidationService emailValidationService,
        StateMachineRunner<AccountState, AccountTrigger, AccountContext> accountWorkflow,
        IAccountContextFactory accountContextFactory)
    {
        _accountContextFactory = accountContextFactory;
        _accountWorkflow = accountWorkflow;
        _emailValidationService = emailValidationService;
        _memberEmailService = memberEmailService;
        _memberPasswordService = memberPasswordService;
        _passwordHasher = passwordHasher;
        _settings = settings;
        _unitOfWork = unitOfWork;
        _unitOfWork = unitOfWork;

        // A throwaway hash used to equalise timing on the login "no such user / no password" path so it
        // costs the same PBKDF2 work as a real check, preventing user enumeration via response time.
        _dummyPassword = new Lazy<IHashedPassword>(() =>
        {
            var (hash, options) = _passwordHasher.ComputeHash("not-a-real-password");
            return new MemberPassword
            {
                Algorithm = options.Algorithm,
                Hash = hash,
                Iterations = options.Iterations,
                Salt = options.Salt
            };
        });
    }

    public async Task<ServiceResult> ActivateChapterAccountAsync(
        IChapterServiceRequest request,
        string activationToken,
        string password)
    {
        var chapter = request.Chapter;

        /* Resolved before the machine, because a link naming no account leaves nothing whose state could be
           read - there is no member, so no transition to pick. */
        var token = await _unitOfWork.MemberActivationTokenRepository
            .GetByToken(activationToken)
            .Run();
        if (token == null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        OdkAssertions.MeetsCondition(token, x => x.ChapterId == chapter.Id);

        var context = await _accountContextFactory.CreateForChapterActivation(request, token, password);

        var result = await _accountWorkflow.Fire(AccountTrigger.Activate, context);

        return result.ToServiceResult();
    }

    public async Task<ServiceResult> ActivateSiteAccountAsync(
        IServiceRequest request,
        string activationToken,
        string password)
    {
        var token = await _unitOfWork.MemberActivationTokenRepository
            .GetByToken(activationToken)
            .Run();

        /* A chapter-scoped token on the site path is refused rather than asserted: the two activation pages
           are separate URLs a member can arrive at, so the wrong one is a bad link, not a bad request. */
        if (token == null || token.ChapterId != null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        var context = await _accountContextFactory.CreateForSiteActivation(request, token, password);

        var result = await _accountWorkflow.Fire(AccountTrigger.Activate, context);

        return result.ToServiceResult();
    }

    public async Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword)
    {
        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(memberId)
            .Run();
        var matches = CheckPassword(memberPassword, currentPassword);
        if (!matches)
        {
            return ServiceResult.Failure("Current password is incorrect");
        }

        var validationResult = await _memberPasswordService.Validate(newPassword);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        memberPassword = _memberPasswordService.Apply(memberPassword, newPassword);
        _unitOfWork.MemberPasswordRepository.Update(memberPassword);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<Member?> GetMemberAsync(string username, string password)
    {
        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(username)
            .Run();
        if (member == null || !member.IsCurrent())
        {
            // Equalise timing with the valid-user path so login response time can't reveal whether an
            // account exists (user enumeration).
            _passwordHasher.Check(password, _dummyPassword.Value);
            return null;
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(member.Id)
            .Run();

        if (!CheckPassword(memberPassword, password))
        {
            if (memberPassword == null)
            {
                // Member exists but has no password set - still perform a hash so timing matches.
                _passwordHasher.Check(password, _dummyPassword.Value);
            }

            return null;
        }

        if (_passwordHasher.ShouldUpdate(memberPassword))
        {
            memberPassword = _memberPasswordService.Apply(memberPassword, password);
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
            await _unitOfWork.SaveChangesAsync();
        }

        await CompleteReferral(member);

        return member;
    }

    public async Task<IReadOnlyCollection<Claim>> GetClaimsAsync(IMemberServiceRequest request)
    {
        var claimsUser = new OdkClaimsUser(request.CurrentMember);
        return claimsUser
            .GetClaims()
            .ToArray();
    }

    public async Task<ServiceResult> RequestPasswordResetAsync(
        IServiceRequest request,
        Chapter? chapter,
        string emailAddress)
    {
        var emailValidationResult = await _emailValidationService.Validate(emailAddress, EmailValidationLevel.Full);
        if (!emailValidationResult.Success)
        {
            return emailValidationResult;
        }

        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(emailAddress)
            .Run();
        if (member == null)
        {
            // return fake success to avoid leaking valid email addresses
            return ServiceResult.Successful();
        }

        if (!member.Activated)
        {
            var activationToken = await _unitOfWork.MemberActivationTokenRepository.GetByMemberId(member.Id).Run();
            if (activationToken == null)
            {
                activationToken = _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
                {
                    ActivationToken = TokenGenerator.GenerateBase64Token(64),
                    ChapterId = chapter?.Id,
                    MemberId = member.Id
                });
                await _unitOfWork.SaveChangesAsync();
            }

            await _memberEmailService.SendActivationEmail(
                request,
                chapter,
                member,
                activationToken.ActivationToken);
            return ServiceResult.Successful();
        }

        var created = DateTime.UtcNow;
        var expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
        var token = TokenGenerator.GenerateBase64Token(64);

        _unitOfWork.MemberPasswordResetRequestRepository.Add(new MemberPasswordResetRequest
        {
            CreatedUtc = created,
            ExpiresUtc = expires,
            MemberId = member.Id,
            Token = token
        });

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendPasswordResetEmail(request, chapter, member, token);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> RequestPasswordResetAsync(
        IServiceRequest request,
        string emailAddress)
    {
        return await RequestPasswordResetAsync(request, null, emailAddress);
    }

    public async Task<ServiceResult> ResetPasswordAsync(string token, string password)
    {
        var validationResult = await _memberPasswordService.Validate(password);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        const string message = "Link is invalid or has expired. Please request a new link using the password reset form.";

        var request = await _unitOfWork.MemberPasswordResetRequestRepository
            .GetByToken(token)
            .Run();
        if (request == null)
        {
            return ServiceResult.Failure(message);
        }

        _unitOfWork.MemberPasswordResetRequestRepository.Delete(request);

        if (request.ExpiresUtc < DateTime.UtcNow)
        {
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Failure(message);
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(request.MemberId)
            .Run();

        memberPassword = _memberPasswordService.Apply(memberPassword, password);

        if (memberPassword.MemberId == default)
        {
            memberPassword.MemberId = request.MemberId;
            _unitOfWork.MemberPasswordRepository.Add(memberPassword);
        }
        else
        {
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// Marks the referral this member signed up from as complete, on their first successful login.
    /// Idempotent by the CompletedUtc check rather than by counting logins, so later logins are a no-op
    /// and the timestamp always records the first one.
    /// </summary>
    private async Task CompleteReferral(Member member)
    {
        if (member.ReferralId == null)
        {
            return;
        }

        var referral = await _unitOfWork.ReferralRepository
            .GetByIdOrDefault(member.ReferralId.Value)
            .Run();
        if (referral == null || referral.CompletedUtc != null)
        {
            return;
        }

        referral.CompletedUtc = DateTime.UtcNow;
        _unitOfWork.ReferralRepository.Update(referral);
        await _unitOfWork.SaveChangesAsync();
    }

    private bool CheckPassword([NotNullWhen(true)] MemberPassword? memberPassword, string password)
    {
        return memberPassword != null
            ? _passwordHasher.Check(password, memberPassword)
            : false;
    }
}
