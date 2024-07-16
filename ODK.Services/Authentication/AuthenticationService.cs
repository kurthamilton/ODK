using System.Security.Claims;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailService _emailService;
    private readonly AuthenticationServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationService(IEmailService emailService, AuthenticationServiceSettings settings,
        IAuthorizationService authorizationService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _emailService = emailService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ActivateAccountAsync(string activationToken, string password)
    {
        var token = await _unitOfWork.MemberActivationTokenRepository
            .GetByToken(activationToken)
            .RunAsync();
        if (token == null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        var (member, memberPassword) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(token.MemberId),
            x => x.MemberPasswordRepository.GetByMemberId(token.MemberId));

        UpdatePassword(memberPassword, password);
        member.Activated = true;
        _unitOfWork.MemberActivationTokenRepository.Delete(token);
        
        await _unitOfWork.SaveChangesAsync();

        await SendNewMemberEmailsAsync(member);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword)
    {
        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(memberId)
            .RunAsync();
        bool matches = CheckPassword(memberPassword, currentPassword);
        if (!matches)
        {
            return ServiceResult.Failure("Current password is incorrect");
        }

        UpdatePassword(memberPassword, newPassword);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<Member?> GetMemberAsync(string username, string password)
    {
        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(username)
            .RunAsync();
        if (member == null)
        {
            return null;
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(member.Id)
            .RunAsync();

        bool passwordMatches = CheckPassword(memberPassword, password);
        return passwordMatches ? member : null;
    }

    public async Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member? member)
    {
        if (member == null)
        {
            return Array.Empty<Claim>();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim("ChapterId", member.ChapterId.ToString())
        };

        var (subscription, adminMembers) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(member.Id),
            x => x.ChapterAdminMemberRepository.GetByMemberId(member.ChapterId));

        bool isActive = await _authorizationService.MembershipIsActiveAsync(subscription, member.ChapterId);
        if (isActive)
        {
            claims.Add(new Claim(ClaimTypes.Role, OdkRoles.Member));

            if (adminMembers.Any())
            {
                claims.Add(new Claim(ClaimTypes.Role, OdkRoles.Admin));
            }

            if (member.SuperAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, OdkRoles.SuperAdmin));
            }
        }

        return claims;
    }
    
    public async Task<ServiceResult> RequestPasswordResetAsync(string emailAddress)
    {
        if (!MailUtils.ValidEmailAddress(emailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(emailAddress)
            .RunAsync();
        if (member == null)
        {
            // return fake success to avoid leaking valid email addresses
            return ServiceResult.Successful();
        }

        DateTime created = DateTime.UtcNow;
        DateTime expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
        string token = RandomStringGenerator.Generate(64);
        
        var chapter = await _unitOfWork.ChapterRepository
            .GetById(member.ChapterId)
            .RunAsync();

        _unitOfWork.MemberPasswordResetRequestRepository.Add(new MemberPasswordResetRequest
        {
            Created = created,
            Expires = expires,
            MemberId = member.Id,
            Token = token
        });

        await _unitOfWork.SaveChangesAsync();

        string url = _settings.PasswordResetUrl.Interpolate(new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "token", HttpUtility.UrlEncode(token) }
        });

        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.PasswordReset, new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "url", url }
        });

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ResetPasswordAsync(string token, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult.Failure("Password cannot be blank");
        }

        const string message = "Link is invalid or has expired. Please request a new link using the password reset form.";

        var request = await _unitOfWork.MemberPasswordResetRequestRepository
            .GetByToken(token)
            .RunAsync();
        if (request == null)
        {
            return ServiceResult.Failure(message);
        }

        _unitOfWork.MemberPasswordResetRequestRepository.Delete(request);

        if (request.Expires < DateTime.UtcNow)
        {
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Failure(message);
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(request.MemberId)
            .RunAsync();

        UpdatePassword(memberPassword, password);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new OdkServiceException("Password cannot be blank");
        }
    }    

    private bool CheckPassword(MemberPassword memberPassword, string password)
    {
        string passwordHash = PasswordHasher.ComputeHash(password, memberPassword.Salt);
        return memberPassword.Password == passwordHash;
    }

    private async Task SendNewMemberEmailsAsync(Member member)
    {
        var (chapter, chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(member.ChapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(member.ChapterId),
            x => x.MemberPropertyRepository.GetByMemberId(member.Id));
        
        string eventsUrl = _settings.EventsUrl.Interpolate(new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name }
        });

        await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.NewMember, new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "eventsUrl", eventsUrl },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) }
        });

        var newMemberAdminEmailParameters = new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "member.emailAddress", HttpUtility.HtmlEncode(member.EmailAddress) },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) },
            { "member.lastName", HttpUtility.HtmlEncode(member.LastName) }
        };

        foreach (var chapterProperty in chapterProperties)
        {
            string? value = memberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id)?.Value;
            newMemberAdminEmailParameters.Add($"member.properties.{chapterProperty.Name}", HttpUtility.HtmlEncode(value ?? ""));
        }

        await _emailService.SendNewMemberAdminEmail(chapter, member, newMemberAdminEmailParameters);
    }

    private void UpdatePassword(MemberPassword memberPassword, string password)
    {
        ValidatePassword(password);

        (string hash, string salt) = PasswordHasher.ComputeHash(password);

        memberPassword.Password = password;
        memberPassword.Salt = salt;
    }
}
