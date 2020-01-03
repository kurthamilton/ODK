using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAuthenticationTokenFactory _authenticationTokenFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailService _emailService;
        private readonly IMemberRepository _memberRepository;
        private readonly AuthenticationServiceSettings _settings;

        public AuthenticationService(IMemberRepository memberRepository, IEmailService emailService, AuthenticationServiceSettings settings,
            IAuthorizationService authorizationService, IChapterRepository chapterRepository,
            IAuthenticationTokenFactory authenticationTokenFactory)
        {
            _authenticationTokenFactory = authenticationTokenFactory;
            _authorizationService = authorizationService;
            _chapterRepository = chapterRepository;
            _emailService = emailService;
            _memberRepository = memberRepository;
            _settings = settings;
        }

        public async Task ActivateAccount(string activationToken, string password)
        {
            MemberActivationToken token = await _memberRepository.GetMemberActivationToken(activationToken);
            if (token == null)
            {
                return;
            }

            await UpdatePassword(token.MemberId, password);

            await _memberRepository.DeleteActivationToken(token.MemberId);
            await _memberRepository.ActivateMember(token.MemberId);

            await SendNewMemberEmails(token.MemberId);
        }

        public async Task ChangePassword(Guid memberId, string currentPassword, string newPassword)
        {
            await _authorizationService.AssertMemberIsCurrent(memberId);
            await AssertMemberPasswordMatches(memberId, currentPassword, "Current password is incorrect");
            await UpdatePassword(memberId, newPassword);
        }

        public async Task DeleteRefreshToken(string refreshToken)
        {
            MemberRefreshToken token = await _memberRepository.GetRefreshToken(refreshToken);
            if (token == null)
            {
                return;
            }

            await _memberRepository.DeleteRefreshToken(token);
        }

        public async Task<AuthenticationToken> Login(string username, string password)
        {
            const string message = "Username or password incorrect";

            Member member = await _memberRepository.FindMemberByEmailAddress(username);

            await AssertMemberPasswordMatches(member?.Id, password, message);

            try
            {
                _authorizationService.AssertMemberIsCurrent(member);
            }
            catch
            {
                if (member?.Disabled == true)
                {
                    throw new OdkServiceException("This account has been disabled");
                }

                throw;
            }

            return await GenerateAccessToken(member);
        }

        public async Task<AuthenticationToken> RefreshToken(string refreshToken)
        {
            const string message = "Invalid token";

            MemberRefreshToken memberRefreshToken = await _memberRepository.GetRefreshToken(refreshToken);
            if (memberRefreshToken == null)
            {
                throw new OdkServiceException(message);
            }

            if (memberRefreshToken.Expires < DateTime.UtcNow)
            {
                await _memberRepository.DeleteRefreshToken(memberRefreshToken);
                throw new OdkServiceException(message);
            }

            Member member = await _memberRepository.GetMember(memberRefreshToken.MemberId);
            _authorizationService.AssertMemberIsCurrent(member);

            AuthenticationToken authenticationToken = await GenerateAccessToken(member, memberRefreshToken.Expires);

            await _memberRepository.DeleteRefreshToken(memberRefreshToken);

            return authenticationToken;
        }

        public async Task RequestPasswordReset(string emailAddress)
        {
            if (!MailUtils.ValidEmailAddress(emailAddress))
            {
                throw new OdkServiceException("Invalid email address format");
            }

            Member member = await _memberRepository.FindMemberByEmailAddress(emailAddress);

            DateTime created = DateTime.UtcNow;
            DateTime expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
            string token = RandomStringGenerator.Generate(64);

            try
            {
                _authorizationService.AssertMemberIsCurrent(member);
            }
            catch
            {
                return;
            }

            Chapter chapter = await _chapterRepository.GetChapter(member.ChapterId);

            await _memberRepository.AddPasswordResetRequest(member.Id, created, expires, token);

            string url = _settings.PasswordResetUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "token", HttpUtility.UrlEncode(token) }
            });

            await _emailService.SendEmail(chapter, member.EmailAddress, EmailType.PasswordReset, new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "url", url }
            });
        }

        public async Task ResetPassword(string token, string password)
        {
            const string message = "Link is invalid or has expired. Please request a new link using the password reset form.";

            MemberPasswordResetRequest request = await _memberRepository.GetPasswordResetRequest(token);
            if (request == null)
            {
                throw new OdkServiceException(message);
            }

            if (request.Expires < DateTime.UtcNow)
            {
                await _memberRepository.DeletePasswordResetRequest(request.Id);
                throw new OdkServiceException(message);
            }

            await UpdatePassword(request.MemberId, password);

            await _memberRepository.DeletePasswordResetRequest(request.Id);
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new OdkServiceException("Password cannot be blank");
            }
        }

        private async Task AssertMemberPasswordMatches(Guid? memberId, string password, string message)
        {
            if (memberId == null)
            {
                throw new OdkServiceException(message);
            }

            MemberPassword memberPassword = await _memberRepository.GetMemberPassword(memberId.Value);
            if (memberPassword == null)
            {
                throw new OdkServiceException(message);
            }

            if (string.IsNullOrWhiteSpace(memberPassword.Password))
            {
                message = "Please use the forgotten password feature to reset your password. " +
                          "The website has been moved to a new system. " +
                          "Your password is stored securely and could not be moved across with the rest of your data.";
                throw new OdkServiceException(message);
            }

            if (PasswordHasher.ComputeHash(password, memberPassword.Salt) != memberPassword.Password)
            {
                throw new OdkServiceException(message);
            }
        }

        private async Task<AuthenticationToken> GenerateAccessToken(Member member, DateTime? refreshTokenExpires = null)
        {
            IReadOnlyCollection<ChapterAdminMember> adminChapterMembers = await _chapterRepository.GetChapterAdminMembersByMember(member.Id);
            MemberSubscription subscription = await _memberRepository.GetMemberSubscription(member.Id);

            string accessToken = _authenticationTokenFactory.Create(member, _settings.AccessTokenLifetimeMinutes);
            string refreshToken = await GenerateRefreshToken(member.Id, refreshTokenExpires);

            return new AuthenticationToken(member.Id, member.ChapterId, accessToken, refreshToken,
                adminChapterMembers.Select(x => x.ChapterId),
                adminChapterMembers.Any(x => x.SuperAdmin),
                subscription.ExpiryDate,
                await _authorizationService.MembershipIsActive(subscription, member.ChapterId));
        }

        private async Task<string> GenerateRefreshToken(Guid memberId, DateTime? expires = null)
        {
            string refreshToken = RandomStringGenerator.Generate(64);

            if (expires == null)
            {
                expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenLifetimeDays);
            }

            MemberRefreshToken token = new MemberRefreshToken(Guid.Empty, memberId, expires.Value, refreshToken);
            await _memberRepository.AddRefreshToken(token);

            return refreshToken;
        }

        private async Task SendNewMemberEmails(Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            Chapter chapter = await _chapterRepository.GetChapter(member.ChapterId);

            string eventsUrl = _settings.EventsUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name }
            });

            await _emailService.SendEmail(chapter, member.EmailAddress, EmailType.NewMember, new Dictionary<string, string>
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
                string value = memberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id)?.Value;
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
