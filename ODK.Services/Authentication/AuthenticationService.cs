using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Authentication
{
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

        public async Task<ServiceResult> ActivateAccount(string activationToken, string password)
        {
            MemberActivationToken? token = await _memberRepository.GetMemberActivationToken(activationToken);
            if (token == null)
            {
                return ServiceResult.Failure("The link you followed is no longer valid");
            }

            await UpdatePassword(token.MemberId, password);

            await _memberRepository.DeleteActivationToken(token.MemberId);
            await _memberRepository.ActivateMember(token.MemberId);

            await SendNewMemberEmails(token.MemberId);

            return ServiceResult.Successful();
        }

        public async Task<ServiceResult> ChangePassword(Guid memberId, string currentPassword, string newPassword)
        {
            bool matches = await CheckPassword(memberId, currentPassword);
            if (!matches)
            {
                return ServiceResult.Failure("Current password is incorrect");
            }

            await UpdatePassword(memberId, newPassword);
            return ServiceResult.Successful();
        }

        public async Task<Member?> GetMember(string username, string password)
        {
            Member? member = await _memberRepository.FindMemberByEmailAddress(username);
            if (member == null)
            {
                return null;
            }

            bool passwordMatches = await CheckPassword(member.Id, password);
            return passwordMatches ? member : null;
        }

        public async Task<IReadOnlyCollection<Claim>> GetClaims(Member? member)
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

            MemberSubscription? subscription = await _memberRepository.GetMemberSubscription(member.Id);

            bool isActive = subscription != null && await _authorizationService.MembershipIsActive(subscription, member.ChapterId);
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
        
        public async Task<ServiceResult> RequestPasswordReset(string emailAddress)
        {
            if (!MailUtils.ValidEmailAddress(emailAddress))
            {
                return ServiceResult.Failure("Invalid email address format");
            }

            Member? member = await _memberRepository.FindMemberByEmailAddress(emailAddress);
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

            await _memberRepository.AddPasswordResetRequest(member.Id, created, expires, token);

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

        public async Task<ServiceResult> ResetPassword(string token, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult.Failure("Password cannot be blank");
            }

            const string message = "Link is invalid or has expired. Please request a new link using the password reset form.";

            MemberPasswordResetRequest? request = await _memberRepository.GetPasswordResetRequest(token);
            if (request == null)
            {
                return ServiceResult.Failure(message);
            }

            if (request.Expires < DateTime.UtcNow)
            {
                await _memberRepository.DeletePasswordResetRequest(request.Id);
                return ServiceResult.Failure(message);
            }

            await UpdatePassword(request.MemberId, password);

            await _memberRepository.DeletePasswordResetRequest(request.Id);

            return ServiceResult.Successful();
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new OdkServiceException("Password cannot be blank");
            }
        }
        

        private async Task<bool> CheckPassword(Guid? memberId, string password)
        {
            if (memberId == null)
            {
                return false;
            }

            MemberPassword? memberPassword = await _memberRepository.GetMemberPassword(memberId.Value);
            if (memberPassword == null)
            {
                return false;
            }

            string passwordHash = PasswordHasher.ComputeHash(password, memberPassword.Salt);
            return memberPassword.Password == passwordHash;
        }

        private async Task SendNewMemberEmails(Guid memberId)
        {
            Member? member = await _memberRepository.GetMember(memberId);
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

            IReadOnlyCollection<MemberProperty> memberProperties = await _memberRepository.GetMemberProperties(memberId);
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);

            IDictionary<string, string> newMemberAdminEmailParameters = new Dictionary<string, string>
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

        private async Task UpdatePassword(Guid memberId, string password)
        {
            ValidatePassword(password);

            (string hash, string salt) = PasswordHasher.ComputeHash(password);

            await _memberRepository.UpdateMemberPassword(new MemberPassword(memberId, hash, salt));
        }
    }
}
