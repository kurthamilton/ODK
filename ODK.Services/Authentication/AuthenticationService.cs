using System.Security.Claims;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IChapterRepository _chapterRepository;
    private readonly IEmailService _emailService;
    private readonly IMemberRepository _memberRepository;
    private readonly AuthenticationServiceSettings _settings;

    public AuthenticationService(IMemberRepository memberRepository, IEmailService emailService, AuthenticationServiceSettings settings,
        IAuthorizationService authorizationService, IChapterRepository chapterRepository)
    {
        _authorizationService = authorizationService;
        _chapterRepository = chapterRepository;
        _emailService = emailService;
        _memberRepository = memberRepository;
        _settings = settings;
    }

    public async Task<ServiceResult> ActivateAccountAsync(string activationToken, string password)
    {
        MemberActivationToken? token = await _memberRepository.GetMemberActivationTokenAsync(activationToken);
        if (token == null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        await UpdatePasswordAsync(token.MemberId, password);

        await _memberRepository.DeleteActivationTokenAsync(token.MemberId);
        await _memberRepository.ActivateMemberAsync(token.MemberId);

        await SendNewMemberEmailsAsync(token.MemberId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword)
    {
        bool matches = await CheckPasswordAsync(memberId, currentPassword);
        if (!matches)
        {
            return ServiceResult.Failure("Current password is incorrect");
        }

        await UpdatePasswordAsync(memberId, newPassword);
        return ServiceResult.Successful();
    }

    public async Task<Member?> GetMemberAsync(string username, string password)
    {
        Member? member = await _memberRepository.FindMemberByEmailAddressAsync(username);
        if (member == null)
        {
            return null;
        }

        bool passwordMatches = await CheckPasswordAsync(member.Id, password);
        return passwordMatches ? member : null;
    }

    public async Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member? member)
    {
        if (member == null)
        {
            return Array.Empty<Claim>();
        }

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim("ChapterId", member.ChapterId.ToString())
        };

        MemberSubscription? subscription = await _memberRepository.GetMemberSubscriptionAsync(member.Id);

        bool isActive = subscription != null && await _authorizationService.MembershipIsActiveAsync(subscription, member.ChapterId);
        if (isActive)
        {
            claims.Add(new Claim(ClaimTypes.Role, OdkRoles.Member));

            IReadOnlyCollection<ChapterAdminMember> adminChapterMembers = await _chapterRepository.GetChapterAdminMembersByMember(member.Id);
            if (adminChapterMembers.Any())
            {
                claims.Add(new Claim(ClaimTypes.Role, OdkRoles.Admin));
            }

            if (adminChapterMembers.Any(x => x.SuperAdmin))
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

        Member? member = await _memberRepository.FindMemberByEmailAddressAsync(emailAddress);
        if (member == null)
        {
            // return fake success to avoid leaking valid email addresses
            return ServiceResult.Successful();
        }

        DateTime created = DateTime.UtcNow;
        DateTime expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
        string token = RandomStringGenerator.Generate(64);
        
        Chapter? chapter = await _chapterRepository.GetChapter(member.ChapterId);
        if (chapter == null)
        {
            throw new OdkNotFoundException();
        }

        await _memberRepository.AddPasswordResetRequestAsync(member.Id, created, expires, token);

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

        MemberPasswordResetRequest? request = await _memberRepository.GetPasswordResetRequestAsync(token);
        if (request == null)
        {
            return ServiceResult.Failure(message);
        }

        if (request.Expires < DateTime.UtcNow)
        {
            await _memberRepository.DeletePasswordResetRequestAsync(request.Id);
            return ServiceResult.Failure(message);
        }

        await UpdatePasswordAsync(request.MemberId, password);

        await _memberRepository.DeletePasswordResetRequestAsync(request.Id);

        return ServiceResult.Successful();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new OdkServiceException("Password cannot be blank");
        }
    }
    

    private async Task<bool> CheckPasswordAsync(Guid? memberId, string password)
    {
        if (memberId == null)
        {
            return false;
        }

        MemberPassword? memberPassword = await _memberRepository.GetMemberPasswordAsync(memberId.Value);
        if (memberPassword == null)
        {
            return false;
        }

        string passwordHash = PasswordHasher.ComputeHash(password, memberPassword.Salt);
        return memberPassword.Password == passwordHash;
    }

    private async Task SendNewMemberEmailsAsync(Guid memberId)
    {
        Member? member = await _memberRepository.GetMemberAsync(memberId);
        if (member == null)
        {
            return;
        }

        Chapter? chapter = await _chapterRepository.GetChapter(member.ChapterId);
        if (chapter == null)
        {
            return;
        }

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

        IReadOnlyCollection<MemberProperty> memberProperties = await _memberRepository.GetMemberPropertiesAsync(memberId);
        IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);

        var newMemberAdminEmailParameters = new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
            { "member.emailAddress", HttpUtility.HtmlEncode(member.EmailAddress) },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) },
            { "member.lastName", HttpUtility.HtmlEncode(member.LastName) }
        };

        foreach (ChapterProperty chapterProperty in chapterProperties)
        {
            string? value = memberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id)?.Value;
            newMemberAdminEmailParameters.Add($"member.properties.{chapterProperty.Name}", HttpUtility.HtmlEncode(value ?? ""));
        }

        await _emailService.SendNewMemberAdminEmail(chapter, member, newMemberAdminEmailParameters);
    }

    private async Task UpdatePasswordAsync(Guid memberId, string password)
    {
        ValidatePassword(password);

        (string hash, string salt) = PasswordHasher.ComputeHash(password);

        await _memberRepository.UpdateMemberPasswordAsync(new MemberPassword(memberId, hash, salt));
    }
}
